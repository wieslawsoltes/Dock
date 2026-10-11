using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutParentPolicyTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Leaving_A_Parent_Clears_Selection_Before_Ownership_Changes(bool tool, bool native)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane previous = new(content);
        LayoutDocumentPane next = new();
        content.IsSelected = true;
        ILayoutContainer? observedParent = null;
        content.IsSelectedChanged += (_, _) => observedParent = content.Parent;

        if (native) content.DockModel.Owner = next.DockModel;
        else content.Parent = next;

        Assert.False(content.IsSelected);
        Assert.Same(previous, observedParent);
        Assert.Same(next, content.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Preselected_Content_Becomes_The_Selected_Child_When_Attached(bool rooted, bool tool)
    {
        LayoutContent first = tool ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tool ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        pane.InsertChildAt(0, first);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutRoot? root = rooted ? new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) } : null;
        LayoutContent added = tool ? new LayoutAnchorable() : new LayoutDocument();
        added.IsSelected = true;

        pane.InsertChildAt(1, added);

        Assert.Same(added, selector.SelectedContent);
        Assert.Equal(1, selector.SelectedContentIndex);
        Assert.True(added.IsSelected);
        Assert.False(first.IsSelected);
        GC.KeepAlive(root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Collection_Observers_See_Attached_Content_And_Updated_Selection(bool replace, bool rooted)
    {
        LayoutDocument original = new();
        LayoutDocumentPane pane = new(original);
        LayoutRoot? root = rooted ? new() { RootPanel = new LayoutPanel(pane) } : null;
        LayoutDocument added = new() { IsSelected = true };
        int notifications = 0;
        pane.Children.CollectionChanged += (_, _) =>
        {
            Assert.Same(pane, added.Parent);
            Assert.Same(added, pane.SelectedContent);
            if (replace) Assert.Null(original.Parent);
            notifications++;
        };

        if (replace) pane.Children[0] = added;
        else pane.Children.Add(added);

        Assert.Equal(1, notifications);
        GC.KeepAlive(root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejected_Collection_Changes_Preserve_The_Existing_Hierarchy(bool replace)
    {
        LayoutDocument original = new();
        LayoutDocumentPane pane = new(original);
        LayoutDocument added = new();
        Exception? error = await Task.Run(() => Record.Exception(() =>
        {
            if (replace) pane.Children[0] = added;
            else pane.Children.Add(added);
        }));

        Assert.IsType<InvalidOperationException>(error);
        Assert.Same(original, Assert.Single(pane.Children));
        Assert.Same(pane, original.Parent);
        Assert.Null(added.Parent);
        Assert.Same(original.DockModel,
            Assert.Single(((Dock.Model.Avalonia.Controls.DocumentDock)pane.DockModel).VisibleDockables!));
    }

    [AvaloniaFact]
    public void Pane_Selection_Synchronizes_In_Both_Directions_Before_A_Factory_Exists()
    {
        LayoutDocument first = new();
        LayoutDocument second = new();
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(second);
        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);

        second.IsSelected = true;
        Assert.Same(second, pane.SelectedContent);
        Assert.False(first.IsSelected);
        pane.SelectedContentIndex = 0;
        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);
        ((Dock.Model.Avalonia.Controls.DocumentDock)pane.DockModel).ActiveDockable = second.DockModel;
        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
    }

    [AvaloniaFact]
    public void Derived_Groups_Can_Set_Visibility_And_Override_The_Notification_Hook()
    {
        VisibilityPane pane = new(new LayoutAnchorable());
        LayoutPanel panel = new(pane);
        pane.Changes.Clear();

        pane.SetVisibility(false);
        pane.SetVisibility(false);
        Assert.False(panel.IsVisible);
        pane.SetVisibility(true);

        Assert.True(panel.IsVisible);
        Assert.Equal(new[] { false, true }, pane.Changes);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void AutoHide_Side_Is_Assigned_Before_The_Parent_Notification(AnchorSide expected)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = new();
        AnchorSide? observed = null;
        side.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(side.Parent)) observed = side.Side;
        };

        switch (expected)
        {
            case AnchorSide.Left: root.LeftSide = side; break;
            case AnchorSide.Top: root.TopSide = side; break;
            case AnchorSide.Right: root.RightSide = side; break;
            case AnchorSide.Bottom: root.BottomSide = side; break;
        }

        Assert.Equal(expected, observed);
        Assert.Same(root, side.Parent);
    }

    [AvaloniaFact]
    public void Tool_Pane_Hosting_Uses_The_Floating_Tree_And_Visible_Pane_Count()
    {
        LayoutAnchorablePane first = new(new LayoutAnchorable());
        LayoutAnchorablePane empty = new();
        LayoutAnchorablePaneGroup nested = new(first);
        LayoutAnchorablePaneGroup outer = new();
        outer.Children.Add(nested);
        outer.Children.Add(empty);
        LayoutAnchorableFloatingWindow floating = new() { RootPanel = outer };

        Assert.True(first.IsHostedInFloatingWindow);
        Assert.True(first.IsDirectlyHostedInFloatingWindow);
        empty.Children.Add(new LayoutAnchorable());
        Assert.False(first.IsDirectlyHostedInFloatingWindow);
        GC.KeepAlive(floating);
    }

    [AvaloniaFact]
    public void Tool_Pane_Stops_Observing_Its_Previous_Parent_After_A_Move()
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable());
        LayoutAnchorablePaneGroup previous = new(pane);
        LayoutAnchorablePaneGroup next = new();
        int changes = 0;
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(pane.IsDirectlyHostedInFloatingWindow)) changes++;
        };

        next.Children.Add(pane);
        Assert.True(changes > 0);
        changes = 0;
        previous.Children.Add(new LayoutAnchorablePane());
        Assert.Equal(0, changes);
        next.Children.Add(new LayoutAnchorablePane());
        Assert.True(changes > 0);
    }

    [AvaloniaFact]
    public void Empty_Document_Pane_Visibility_Depends_On_Its_Parent()
    {
        LayoutDocumentPane pane = new();
        LayoutPanel panel = new(pane);
        LayoutDocumentPaneGroup group = new();
        List<bool> changes = new();
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(pane.IsVisible)) changes.Add(pane.IsVisible);
        };
        Assert.True(pane.IsVisible);

        group.Children.Add(pane);
        Assert.False(pane.IsVisible);
        Assert.True(group.IsVisible);
        panel.Children.Add(pane);

        Assert.True(pane.IsVisible);
        Assert.Equal(new[] { false, true }, changes);
    }

    [AvaloniaFact]
    public void Visibility_Propagates_Through_Nested_Tool_Groups()
    {
        LayoutAnchorablePane pane = new();
        LayoutAnchorablePaneGroup group = new(pane);
        LayoutPanel panel = new(group);
        Assert.False(pane.IsVisible);
        Assert.False(group.IsVisible);
        Assert.False(panel.IsVisible);
        List<bool> panelChanges = new();
        panel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(panel.IsVisible)) panelChanges.Add(panel.IsVisible);
        };
        LayoutAnchorable tool = new();

        pane.Children.Add(tool);
        Assert.True(pane.IsVisible);
        Assert.True(group.IsVisible);
        Assert.True(panel.IsVisible);
        pane.Children.Remove(tool);

        Assert.False(pane.IsVisible);
        Assert.False(group.IsVisible);
        Assert.False(panel.IsVisible);
        Assert.Equal(new[] { true, false }, panelChanges);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Subtree_Wrappers_Start_With_Computed_Visibility(bool nested)
    {
        LayoutRoot root = new();
        ProportionalDock destination = (ProportionalDock)root.RootPanel.DockModel;
        ToolDock nativePane = new() { VisibleDockables = new AvaloniaList<IDockable>() };
        IDockable added = nativePane;
        if (nested)
        {
            ProportionalDock nativeGroup = new() { VisibleDockables = new AvaloniaList<IDockable>(nativePane) };
            nativePane.Owner = nativeGroup;
            added = nativeGroup;
        }
        added.Owner = destination;

        destination.VisibleDockables!.Add(added);

        ILayoutPanelElement wrapped = root.RootPanel.Children[1];
        Assert.False(wrapped.IsVisible);
        LayoutAnchorablePane pane = nested
            ? Assert.IsType<LayoutAnchorablePane>(Assert.Single(Assert.IsType<LayoutPanel>(wrapped).Children))
            : Assert.IsType<LayoutAnchorablePane>(wrapped);
        Assert.False(pane.IsVisible);
        pane.Children.Add(new LayoutAnchorable());
        Assert.True(pane.IsVisible);
        Assert.True(wrapped.IsVisible);
    }

    [AvaloniaFact]
    public void Group_Visibility_Changes_Notify_Once_And_Expose_Old_Value_First()
    {
        LayoutAnchorablePane pane = new();
        List<(string Phase, bool Visible)> changes = new();
        pane.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == nameof(pane.IsVisible)) changes.Add(("changing", pane.IsVisible));
        };
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(pane.IsVisible)) changes.Add(("changed", pane.IsVisible));
        };
        Assert.True(pane.IsVisible);

        pane.ComputeVisibility();
        pane.ComputeVisibility();

        Assert.False(pane.IsVisible);
        Assert.Equal(new[] { ("changing", true), ("changed", false) }, changes);
    }

    [AvaloniaFact]
    public void Tool_Parent_Changes_Notify_Visibility_Before_The_Parent_Event_Even_When_Still_Visible()
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane previous = new(tool);
        LayoutAnchorablePane next = new();
        List<string> phases = new();
        tool.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(tool.IsVisible) or nameof(tool.IsHidden) or
                nameof(tool.IsAutoHidden) or nameof(tool.Parent)) phases.Add(args.PropertyName);
        };
        tool.IsVisibleChanged += (_, _) =>
        {
            Assert.Same(next, tool.Parent);
            Assert.True(tool.IsVisible);
            phases.Add("visible-event");
        };

        tool.Parent = next;

        Assert.Equal(new[] { "IsVisible", "visible-event", "IsHidden", "IsAutoHidden", "Parent" }, phases);
        GC.KeepAlive(previous);
    }

    private sealed class VisibilityPane : LayoutAnchorablePane
    {
        internal VisibilityPane(LayoutAnchorable content) : base(content) { }
        internal List<bool> Changes { get; } = new();
        internal void SetVisibility(bool value) => IsVisible = value;
        protected override void OnIsVisibleChanged()
        {
            Changes.Add(IsVisible);
            base.OnIsVisibleChanged();
        }
    }
}
