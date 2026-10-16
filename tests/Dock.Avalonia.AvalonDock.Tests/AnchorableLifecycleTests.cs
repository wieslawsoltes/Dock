using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AnchorableLifecycleTests
{
    [AvaloniaFact]
    public void Hide_Honors_Cancellation_And_Explicit_Visibility_Restores_Tab_Order()
    {
        ObservedAnchorable tool = new() { CanHide = false };
        LayoutAnchorablePane pane = new(tool);
        LayoutAnchorable second = new();
        pane.Children.Add(second);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        CancelEventHandler cancel = (_, args) => args.Cancel = true;
        tool.Hiding += cancel.Invoke;
        tool.Hide();
        Assert.True(tool.IsVisible);
        Assert.Equal(1, tool.HidingCalls);
        tool.Hiding -= cancel.Invoke;
        tool.IsVisible = false;
        Assert.True(tool.IsHidden);
        Assert.False(tool.IsVisible);
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        Assert.Equal(1, tool.HiddenCalls);
        Assert.Same(pane, tool.PreviousContainer);
        Assert.Equal(0, tool.PreviousContainerIndex);
        tool.IsVisible = true;
        Assert.Equal(new LayoutAnchorable[] { tool, second }, pane.Children);
        Assert.True(tool.IsSelected);
        Assert.True(tool.IsActive);
        Assert.Null(tool.PreviousContainer);
        Assert.Throws<InvalidOperationException>(() => new LayoutAnchorable().Show());
    }

    [AvaloniaFact]
    public void Non_Cancelable_Hide_Skips_Hiding_Callback()
    {
        ObservedAnchorable tool = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutAnchorablePane(tool)) };
        tool.Hiding += (_, args) => args.Cancel = true;
        tool.Hide(false);
        Assert.True(tool.IsHidden);
        Assert.Equal(0, tool.HidingCalls);
        Assert.Equal(1, tool.HiddenCalls);
        Assert.Same(root, tool.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Show_Calls_Strategy_And_Allows_Custom_Placement(bool handlesInsertion)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane original = new(tool);
        LayoutAnchorablePane destination = new();
        LayoutPanel panel = new(original);
        panel.Children.Add(destination);
        InsertionStrategy strategy = new(destination, handlesInsertion);
        DockingManager manager = new()
        {
            Layout = new LayoutRoot { RootPanel = panel },
            LayoutUpdateStrategy = strategy
        };
        tool.Hide();
        tool.Show();
        Assert.Same(original, strategy.ProposedContainer);
        Assert.Equal(new[] { "before", "after" }, strategy.Calls);
        Assert.Same(handlesInsertion ? destination : original, tool.Parent);
        Assert.Empty(manager.Layout.Hidden);
        Assert.Same(manager.Layout, tool.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Hiding_Auto_Hidden_Tool_Preserves_Group_Across_Save_And_Show(bool roundTrip)
    {
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new object() };
        LayoutAnchorablePane pane = new(tool);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        tool.ToggleAutoHide();
        LayoutAnchorGroup group = Assert.IsType<LayoutAnchorGroup>(tool.Parent);
        tool.Hide();
        Assert.True(tool.IsHidden);
        Assert.Same(group, tool.PreviousContainer);
        Assert.Same(manager.Layout, group.Root);
        if (roundTrip)
        {
            XmlLayoutSerializer serializer = new(manager);
            StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            tool = Assert.Single(manager.Layout.Hidden);
        }
        tool.Show();
        Assert.True(tool.IsAutoHidden);
        Assert.IsType<LayoutAnchorGroup>(tool.Parent);
        Assert.Empty(manager.Layout.Hidden);
        Assert.Same(manager.Layout, tool.Root);
        tool.ToggleAutoHide();
        Assert.Same(Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorablePane>()), tool.Parent);
    }

    [AvaloniaTheory]
    [InlineData(AnchorableShowStrategy.Left, AnchorSide.Left)]
    [InlineData(AnchorableShowStrategy.Right, AnchorSide.Right)]
    [InlineData(AnchorableShowStrategy.Top, AnchorSide.Top)]
    [InlineData(AnchorableShowStrategy.Bottom, AnchorSide.Bottom)]
    public void AddToLayout_Creates_Then_Reuses_Requested_Edge(AnchorableShowStrategy strategy, AnchorSide side)
    {
        DockingManager manager = new();
        LayoutAnchorable first = new();
        first.AddToLayout(manager, strategy);
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(first.Parent);
        Assert.Equal(side, pane.GetSide());
        LayoutAnchorable second = new();
        second.AddToLayout(manager, strategy);
        Assert.Same(pane, second.Parent);
        Assert.Equal(2, pane.Children.Count);
        Assert.Same(manager.DockFactory, second.DockModel.Factory);
        Assert.Throws<InvalidOperationException>(() => first.AddToLayout(manager, strategy));
        second.Hide();
        Assert.Throws<InvalidOperationException>(() => second.AddToLayout(manager, strategy));
    }

    [AvaloniaFact]
    public void Most_Adds_An_Outer_Pane_And_Preserves_Existing_Perpendicular_Splits()
    {
        LayoutDocumentPane first = new(new LayoutDocument());
        LayoutDocumentPane second = new(new LayoutDocument());
        LayoutPanel inner = new(first) { Orientation = Orientation.Vertical };
        inner.Children.Add(second);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = inner } };
        LayoutAnchorable tool = new();
        tool.AddToLayout(manager, AnchorableShowStrategy.Most | AnchorableShowStrategy.Left);
        Assert.Equal(Orientation.Horizontal, manager.Layout.RootPanel.Orientation);
        Assert.Same(inner, manager.Layout.RootPanel.Children[1]);
        Assert.Equal(new[] { first, second }, inner.Children);
        Assert.Equal(Orientation.Vertical, inner.Orientation);
        Assert.Same(manager.Layout, first.Root);
        Assert.Same(tool.Parent, manager.Layout.RootPanel.Children[0]);
    }

    [AvaloniaFact]
    public void Moving_Tool_To_Detached_Auto_Hide_Group_Updates_Visibility_Notifications()
    {
        LayoutAnchorable tool = new();
        List<string?> changed = new();
        int visibleChanges = 0;
        tool.PropertyChanged += (_, args) => changed.Add(args.PropertyName);
        tool.IsVisibleChanged += (_, _) => visibleChanges++;
        LayoutAnchorGroup group = new();
        group.Children.Add(tool);
        Assert.True(tool.IsAutoHidden);
        Assert.True(tool.IsVisible);
        Assert.Contains(nameof(LayoutAnchorable.IsAutoHidden), changed);
        Assert.Equal(1, visibleChanges);
        group.Children.Remove(tool);
        Assert.False(tool.IsAutoHidden);
        Assert.False(tool.IsVisible);
        Assert.Equal(2, visibleChanges);
    }

    private sealed class ObservedAnchorable : LayoutAnchorable
    {
        internal int HidingCalls { get; private set; }
        internal int HiddenCalls { get; private set; }
        protected override void OnHiding(CancelEventArgs args) { HidingCalls++; base.OnHiding(args); }
        protected override void OnHidden() { HiddenCalls++; base.OnHidden(); }
    }

    private sealed class InsertionStrategy(LayoutAnchorablePane destination, bool handlesInsertion) : ILayoutUpdateStrategy
    {
        internal List<string> Calls { get; } = new();
        internal ILayoutContainer? ProposedContainer { get; private set; }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? proposed)
        {
            Calls.Add("before");
            ProposedContainer = proposed;
            if (handlesInsertion) destination.Children.Add(tool);
            return handlesInsertion;
        }
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => Calls.Add("after");
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument tool, ILayoutContainer? proposed) => false;
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument tool) { }
    }
}
