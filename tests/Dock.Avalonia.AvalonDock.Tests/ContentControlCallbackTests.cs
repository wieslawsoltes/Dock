using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentControlCallbackTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Assigning_And_Clearing_A_Model_Does_Not_Copy_Its_Initial_Enablement(bool tool, bool enabled)
    {
        var (manager, first, _) = Create(tool);
        first.IsEnabled = !enabled;
        IProbe probe = NewProbe(tool);
        probe.Control.IsEnabled = enabled;

        probe.Model = first;

        Assert.Equal(enabled, probe.Control.IsEnabled);
        Assert.Same(manager.GetLayoutItemFromModel(first), probe.Item);
        first.IsEnabled = enabled;
        first.IsEnabled = !enabled;
        Assert.Equal(!enabled, probe.Control.IsEnabled);
        probe.Model = null;
        Assert.Null(probe.Item);
        Assert.Equal(!enabled, probe.Control.IsEnabled);
        first.IsEnabled = enabled;
        Assert.Equal(!enabled, probe.Control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unmanaged_Document_Fails_After_Subscription_Without_Replacing_The_Previous_Item(bool hasRoot)
    {
        var (manager, first, _) = Create(false);
        DocumentProbe probe = new() { Model = first };
        LayoutItem item = probe.LayoutItem!;
        LayoutDocument replacement = new();
        LayoutRoot? root = hasRoot ? new() { RootPanel = new(new LayoutDocumentPane(replacement)) } : null;

        Assert.Throws<NullReferenceException>(() => probe.Model = replacement);

        Assert.Same(replacement, probe.Model);
        Assert.Same(item, probe.LayoutItem);
        first.IsEnabled = false;
        Assert.True(probe.IsEnabled);
        replacement.IsEnabled = false;
        Assert.False(probe.IsEnabled);
        replacement.IsEnabled = true;
        Assert.True(probe.IsEnabled);
        probe.Model = null;
        Assert.Null(probe.LayoutItem);
        replacement.IsEnabled = false;
        Assert.True(probe.IsEnabled);
        GC.KeepAlive(manager);
        GC.KeepAlive(root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unmanaged_Tool_Clears_The_Previous_Item_Without_Subscribing(bool hasRoot)
    {
        var (manager, first, _) = Create(true);
        ToolProbe probe = new() { Model = (LayoutAnchorable)first };
        LayoutAnchorable replacement = new();
        LayoutRoot? root = hasRoot ? new() { RootPanel = new(new LayoutAnchorablePane(replacement)) } : null;

        probe.Model = replacement;

        Assert.Null(probe.LayoutItem);
        first.IsEnabled = false;
        replacement.IsEnabled = false;
        Assert.True(probe.IsEnabled);
        GC.KeepAlive(manager);
        GC.KeepAlive(root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Attaching_A_Public_Control_Does_Not_Refresh_An_Explicit_Or_Suppressed_Item(bool tool, bool suppressed)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        LayoutItem expected = suppressed ? probe.Item! : new LayoutDocumentItem();
        if (suppressed)
        {
            probe.Suppress = true;
            probe.Model = second;
        }
        else probe.SetItem(expected);
        Window window = new() { Content = probe.Control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(expected, probe.Item);
        }
        finally { window.Close(); }
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replayed_Callback_Removes_Its_Supplied_Old_Model_And_Observes_The_Current_Model(bool tool)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        AvaloniaPropertyChangedEventArgs initial = probe.Last!;
        probe.Suppress = true;
        probe.Model = second;
        second.IsEnabled = false;
        probe.Apply(initial);
        probe.Control.IsEnabled = true;

        first.IsEnabled = false;

        Assert.False(probe.Control.IsEnabled);
        Assert.Same(second, probe.Model);
        Assert.Same(manager.GetLayoutItemFromModel(second), probe.Item);
        second.IsEnabled = true;
        Assert.True(probe.Control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Suppressed_Model_Hook_Leaves_Old_Events_Reading_The_Live_Property(bool tool)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        probe.Suppress = true;
        second.IsEnabled = false;
        probe.Model = second;
        probe.Control.IsEnabled = true;
        first.IsEnabled = false;
        Assert.False(probe.Control.IsEnabled);
        Assert.Same(manager.GetLayoutItemFromModel(first), probe.Item);
        probe.Model = null;
        probe.Control.IsEnabled = true;
        first.IsEnabled = true;
        first.IsEnabled = false;
        Assert.True(probe.Control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public void Repeated_Base_Calls_Add_Distinct_Enablement_Registrations(bool tool, int registrations)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = second;
        for (int i = 1; i < registrations; i++) probe.Apply(probe.Last!);
        second.IsActive = true;
        ILayoutContentSelector selector = (ILayoutContentSelector)second.Parent!;
        List<int> observed = new();
        ((LayoutElement)second.Parent!).PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ILayoutContentSelector.SelectedContentIndex)) observed.Add(selector.SelectedContentIndex);
        };

        second.IsEnabled = false;

        List<int> expected = new();
        // Clearing the selected child's flag recursively clears its pane index;
        // both the nested and outer index notifications report that empty state.
        for (int i = 0; i < registrations; i++) { expected.Add(-1); expected.Add(-1); expected.Add(0); }
        Assert.Equal(expected, observed);
        Assert.Same(first, selector.SelectedContent);
        Assert.Same(second, manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void A_Callback_Removes_Only_One_Registration_From_Its_Old_Model(bool tool, int registrations)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        for (int i = 1; i < registrations; i++) probe.Apply(probe.Last!);
        probe.Model = second;
        second.IsEnabled = false;
        probe.Control.IsEnabled = true;

        first.IsEnabled = false;

        Assert.Equal(registrations == 1, probe.Control.IsEnabled);
        Assert.Same(manager.GetLayoutItemFromModel(second), probe.Item);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Is_Cleared_Before_The_Next_Enabled_Child_Is_Examined(bool tool)
    {
        var (_, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = second;
        second.IsActive = true;
        ILayoutContentSelector selector = (ILayoutContentSelector)second.Parent!;
        ((LayoutElement)second.Parent!).PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ILayoutContentSelector.SelectedContentIndex) && selector.SelectedContentIndex == -1)
                first.IsEnabled = false;
        };

        second.IsEnabled = false;

        Assert.Equal(-1, selector.SelectedContentIndex);
        Assert.Null(selector.SelectedContent);
        Assert.False(first.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Control_Kind_Limits_The_Pane_Whose_Selection_It_Changes(bool toolControl)
    {
        LayoutAnchorable first = new() { Content = new object() };
        LayoutAnchorable active = new() { Content = new object() };
        ILayoutPanelElement pane = toolControl
            ? new LayoutDocumentPane { Children = { first, active } }
            : new LayoutAnchorablePane { Children = { first, active } };
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        IProbe probe = NewProbe(toolControl);
        probe.Model = active;
        active.IsActive = true;
        active.IsEnabled = false;
        Assert.False(probe.Control.IsEnabled);
        Assert.Same(active, ((ILayoutContentSelector)pane).SelectedContent);
        Assert.Same(active, manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Container_Callback_Failure_Retains_The_Already_Added_Enablement_Registration(bool tool)
    {
        var (manager, first, _) = Create(tool);
        IProbe probe = NewProbe(tool);
        EventHandler<AvaloniaPropertyChangedEventArgs> fail = (_, args) =>
        {
            if (args.Property.Name == nameof(LayoutDocumentControl.LayoutItem)) throw new InvalidOperationException("container");
        };
        probe.Control.PropertyChanged += fail;
        Assert.Throws<InvalidOperationException>(() => probe.Model = first);
        probe.Control.PropertyChanged -= fail;
        Assert.Same(manager.GetLayoutItemFromModel(first), probe.Item);
        first.IsEnabled = false;
        Assert.False(probe.Control.IsEnabled);
        first.IsEnabled = true;
        Assert.True(probe.Control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Old_Model_Registrations_Do_Not_Retain_Discarded_Controls(bool tool)
    {
        var (manager, first, second) = Create(tool);
        WeakReference reference = DiscardControl(tool, first, second);
        for (int i = 0; i < 8 && reference.IsAlive; i++)
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        }
        Assert.False(reference.IsAlive);
        first.IsEnabled = false;
        second.IsEnabled = false;
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Rootless_Tools_Begin_Observing_Enablement_When_The_Hook_Is_Replayed_After_Attachment(bool tool)
    {
        LayoutContent model = tool ? new LayoutAnchorable() : new LayoutDocument();
        IProbe probe = NewProbe(tool);
        if (tool) probe.Model = model;
        else Assert.Throws<NullReferenceException>(() => probe.Model = model);
        model.IsEnabled = false;
        Assert.Equal(tool, probe.Control.IsEnabled);
        ILayoutPanelElement pane = tool ? new LayoutAnchorablePane((LayoutAnchorable)model) : new LayoutDocumentPane(model);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        model.IsEnabled = true;
        model.IsEnabled = false;
        Assert.Equal(tool, probe.Control.IsEnabled);
        Assert.Null(probe.Item);
        probe.Apply(probe.Last!);
        Assert.Equal(tool, probe.Control.IsEnabled);
        Assert.Same(manager.GetLayoutItemFromModel(model), probe.Item);
        model.IsEnabled = true;
        Assert.True(probe.Control.IsEnabled);
        model.IsEnabled = false;
        Assert.False(probe.Control.IsEnabled);
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Enablement_Callback_Redirection_Uses_The_New_Active_Models_Pane(bool tool)
    {
        var (firstManager, first, _) = Create(tool);
        var (secondManager, enabled, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        first.IsActive = true;
        second.IsEnabled = false;
        probe.Control.PropertyChanged += (_, args) =>
        {
            if (args.Property != InputElement.IsEnabledProperty || probe.Control.IsEnabled || !ReferenceEquals(probe.Model, first)) return;
            second.IsActive = true;
            probe.Model = second;
        };
        first.IsEnabled = false;
        Assert.Same(second, probe.Model);
        Assert.Same(enabled, ((ILayoutContentSelector)second.Parent!).SelectedContent);
        Assert.Same(second, secondManager.Layout.ActiveContent);
        GC.KeepAlive(firstManager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Reset_Failure_Stops_Fallback_And_A_Later_Enablement_Change_Can_Retry(bool tool)
    {
        var (manager, first, second) = Create(tool);
        IProbe probe = NewProbe(tool);
        probe.Model = second;
        second.IsActive = true;
        ILayoutContentSelector selector = (ILayoutContentSelector)second.Parent!;
        LayoutElement pane = (LayoutElement)second.Parent!;
        System.ComponentModel.PropertyChangedEventHandler fail = (_, args) =>
        {
            if (args.PropertyName == nameof(ILayoutContentSelector.SelectedContentIndex)) throw new InvalidOperationException("selection");
        };
        pane.PropertyChanged += fail;
        Assert.Throws<InvalidOperationException>(() => second.IsEnabled = false);
        Assert.False(probe.Control.IsEnabled);
        Assert.Equal(-1, selector.SelectedContentIndex);
        pane.PropertyChanged -= fail;
        second.IsEnabled = true;
        second.IsEnabled = false;
        Assert.Same(first, selector.SelectedContent);
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_A_Public_Title_Model_Retains_Its_Container_Across_Attachment(bool clear)
    {
        var (manager, first, _) = Create(true);
        AnchorablePaneTitle title = new() { Model = (LayoutAnchorable)first };
        LayoutItem item = title.LayoutItem!;
        if (clear) title.ClearValue(AnchorablePaneTitle.ModelProperty);
        else title.Model = null;
        Assert.Null(title.Model);
        Assert.Same(item, title.LayoutItem);
        Window window = new() { Content = title };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(item, title.LayoutItem);
        }
        finally { window.Close(); }
        GC.KeepAlive(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference DiscardControl(bool tool, LayoutContent first, LayoutContent second)
    {
        IProbe probe = NewProbe(tool);
        probe.Model = first;
        probe.Apply(probe.Last!);
        probe.Suppress = true;
        probe.Model = second;
        probe.Apply(probe.Last!);
        return new WeakReference(probe);
    }

    private static (DockingManager, LayoutContent, LayoutContent) Create(bool tool)
    {
        LayoutContent first = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent second = tool ? new LayoutAnchorable() : new LayoutDocument();
        first.Content = new object(); second.Content = new object();
        ILayoutPanelElement pane = tool
            ? new LayoutAnchorablePane { Children = { (LayoutAnchorable)first, (LayoutAnchorable)second } }
            : new LayoutDocumentPane { Children = { first, second } };
        return (new() { Layout = new() { RootPanel = new(pane) } }, first, second);
    }

    private static IProbe NewProbe(bool tool) => tool ? new ToolProbe() : new DocumentProbe();

    private interface IProbe
    {
        TemplatedControl Control { get; }
        LayoutContent? Model { get; set; }
        LayoutItem? Item { get; }
        bool Suppress { get; set; }
        AvaloniaPropertyChangedEventArgs? Last { get; }
        void Apply(AvaloniaPropertyChangedEventArgs change);
        void SetItem(LayoutItem? item);
    }

    private sealed class DocumentProbe : LayoutDocumentControl, IProbe
    {
        public TemplatedControl Control => this;
        public LayoutItem? Item => LayoutItem;
        public bool Suppress { get; set; }
        public AvaloniaPropertyChangedEventArgs? Last { get; private set; }
        public void Apply(AvaloniaPropertyChangedEventArgs change) => base.OnModelChanged(change);
        public void SetItem(LayoutItem? item) => SetLayoutItem(item);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Last = e;
            if (!Suppress) base.OnModelChanged(e);
        }
    }

    private sealed class ToolProbe : LayoutAnchorableControl, IProbe
    {
        public TemplatedControl Control => this;
        LayoutContent? IProbe.Model { get => Model; set => Model = (LayoutAnchorable?)value; }
        public LayoutItem? Item => LayoutItem;
        public bool Suppress { get; set; }
        public AvaloniaPropertyChangedEventArgs? Last { get; private set; }
        public void Apply(AvaloniaPropertyChangedEventArgs change) => base.OnModelChanged(change);
        public void SetItem(LayoutItem? item) => SetLayoutItem(item);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Last = e;
            if (!Suppress) base.OnModelChanged(e);
        }
    }
}
