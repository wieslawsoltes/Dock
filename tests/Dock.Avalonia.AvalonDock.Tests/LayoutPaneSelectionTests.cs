using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutPaneSelectionTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Detached_Panes_Select_The_First_Enabled_Content(bool tools)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent disabled = CreateContent(tools);
        disabled.IsEnabled = false;
        pane.InsertChildAt(0, disabled);
        Assert.Null(selector.SelectedContent);
        Assert.Equal(-1, selector.SelectedContentIndex);
        Assert.False(disabled.IsSelected);

        LayoutContent enabled = CreateContent(tools);
        pane.InsertChildAt(1, enabled);

        Assert.Same(enabled, selector.SelectedContent);
        Assert.True(enabled.IsSelected);
        Assert.False(disabled.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Removal_Selects_The_First_Enabled_Remaining_Tool(bool native)
    {
        LayoutAnchorable disabled = new() { IsEnabled = false };
        LayoutAnchorable removed = new();
        LayoutAnchorable expected = new();
        LayoutAnchorablePane pane = new(disabled);
        pane.Children.Add(removed);
        pane.Children.Add(expected);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        pane.SelectedContentIndex = 1;

        if (native) root.DockFactory.RemoveDockable(removed.DockModel, false);
        else pane.Children.Remove(removed);

        Assert.Same(expected, pane.SelectedContent);
        Assert.True(expected.IsSelected);
        Assert.False(disabled.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Rooted_Document_Removal_Uses_Activation_History(bool native)
    {
        LayoutDocument older = new();
        LayoutDocument removed = new();
        LayoutDocument recent = new();
        LayoutDocumentPane pane = new(older);
        pane.Children.Add(removed);
        pane.Children.Add(recent);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        removed.IsActive = true;
        older.LastActivationTimeStamp = new DateTime(2000, 1, 1);
        recent.LastActivationTimeStamp = new DateTime(2001, 1, 1);

        if (native) root.DockFactory.RemoveDockable(removed.DockModel, false);
        else pane.Children.Remove(removed);

        Assert.Same(recent, pane.SelectedContent);
        Assert.Same(recent, root.ActiveContent);
        Assert.True(recent.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Reordering_Applies_The_Panes_Collection_Policy_Before_The_Child_Move_Hook(bool tools, bool moveSelected)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent first = CreateContent(tools);
        LayoutContent selected = CreateContent(tools);
        pane.InsertChildAt(0, first);
        pane.InsertChildAt(1, selected);
        LayoutContent third = CreateContent(tools);
        pane.InsertChildAt(2, third);
        selector.SelectedContentIndex = 1;
        List<(string Phase, int Index)> indices = new();
        pane.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == nameof(selector.SelectedContentIndex)) indices.Add(("changing", selector.SelectedContentIndex));
        };
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(selector.SelectedContentIndex)) indices.Add(("changed", selector.SelectedContentIndex));
        };

        ((ILayoutPane)pane).MoveChild(moveSelected ? 1 : 2, moveSelected ? 2 : 0);

        Assert.Same(moveSelected ? selected : tools ? third : first, selector.SelectedContent);
        Assert.True(selected.IsSelected);
        Assert.Equal(moveSelected ? 2 : tools ? 0 : 1, selector.SelectedContentIndex);
        Assert.Equal(moveSelected
            ? new[] { ("changing", 1), ("changed", 2) }
            : tools
                ? new[] { ("changing", 1), ("changed", 2), ("changing", 2), ("changed", 0) }
                : Array.Empty<(string, int)>(), indices);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_Selection_Changes_Do_Not_Activate_Another_Content(bool tools)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent active = CreateContent(tools);
        LayoutContent selected = CreateContent(tools);
        pane.InsertChildAt(0, active);
        pane.InsertChildAt(1, selected);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        active.IsActive = true;

        selector.SelectedContentIndex = 1;

        Assert.Same(selected, selector.SelectedContent);
        Assert.Same(active, root.ActiveContent);
        Assert.True(selected.IsSelected);
        Assert.False(active.IsSelected);
        selector.SelectedContentIndex = -1;
        Assert.Null(selector.SelectedContent);
        Assert.Same(active, root.ActiveContent);
        selected.IsSelected = true;
        Assert.Same(selected, selector.SelectedContent);
        Assert.Same(active, root.ActiveContent);
        selected.IsSelected = false;
        Assert.Null(selector.SelectedContent);
        Assert.Same(active, root.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_History_Ties_Use_Child_Order_Without_Filtering_Disabled_Content(bool disabled)
    {
        LayoutDocument first = new() { IsEnabled = !disabled };
        LayoutDocument removed = new();
        LayoutDocument other = new();
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(removed);
        pane.Children.Add(other);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        removed.IsActive = true;
        first.LastActivationTimeStamp = other.LastActivationTimeStamp = new DateTime(2000, 1, 1);

        pane.Children.Remove(removed);

        Assert.Same(first, pane.SelectedContent);
        Assert.Same(first, root.ActiveContent);
    }

    [AvaloniaFact]
    public void Queued_Native_Reconciliation_Does_Not_Retain_A_Removed_Pane()
    {
        LayoutRoot retained = new();
        WeakReference<LayoutDocumentPane> reference = RemovePaneWithPendingSelection(retained);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.TryGetTarget(out _));
        GC.KeepAlive(retained);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<LayoutDocumentPane> RemovePaneWithPendingSelection(LayoutRoot root)
    {
        LayoutDocument first = new();
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(new LayoutDocument());
        root.RootPanel.Children.Add(pane);
        ((DocumentDock)pane.DockModel).VisibleDockables!.Remove(first.DockModel);
        root.RootPanel.Children.Remove(pane);
        return new(pane);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Notifications_Bracket_Updated_Native_Selection_Flags(bool tools)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent first = CreateContent(tools);
        LayoutContent second = CreateContent(tools);
        pane.InsertChildAt(0, first);
        pane.InsertChildAt(1, second);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        selector.SelectedContentIndex = 0;
        List<string> phases = new();
        pane.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName is not ("SelectedContentIndex" or "SelectedContent")) return;
            Assert.Same(first, selector.SelectedContent);
            Assert.False(second.IsSelected);
            phases.Add($"changing:{args.PropertyName}:{selector.SelectedContentIndex}");
        };
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not ("SelectedContentIndex" or "SelectedContent")) return;
            Assert.False(first.IsSelected);
            if (selector.SelectedContentIndex == -1)
            {
                Assert.Null(selector.SelectedContent);
                Assert.False(second.IsSelected);
            }
            else
            {
                Assert.Same(second, selector.SelectedContent);
                Assert.True(second.IsSelected);
            }
            phases.Add($"changed:{args.PropertyName}:{selector.SelectedContentIndex}");
        };

        selector.SelectedContentIndex = 1;

        Assert.Equal(new[]
        {
            "changing:SelectedContentIndex:0", "changing:SelectedContent:0",
            "changing:SelectedContentIndex:0", "changing:SelectedContent:0",
            "changed:SelectedContentIndex:-1", "changed:SelectedContent:-1",
            "changed:SelectedContentIndex:1", "changed:SelectedContent:1"
        }, phases);
        GC.KeepAlive(root);
    }

    [AvaloniaFact]
    public void Tool_Panes_Notify_Aggregate_Capabilities_When_Their_Children_Change()
    {
        LayoutAnchorablePane pane = new();
        Assert.True(pane.CanClose);
        Assert.True(pane.CanHide);
        List<string> changes = new();
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is "CanClose" or "CanHide") changes.Add(args.PropertyName);
        };
        LayoutAnchorable tool = new() { CanClose = false, CanHide = false };

        pane.Children.Add(tool);
        Assert.False(pane.CanClose);
        Assert.False(pane.CanHide);
        pane.Children.Remove(tool);

        Assert.True(pane.CanClose);
        Assert.True(pane.CanHide);
        Assert.Equal(new[] { "CanClose", "CanHide", "CanClose", "CanHide" }, changes);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Derived_Panes_Receive_The_Child_Move_Hook_After_Reordering(bool tools)
    {
        MoveDocumentPane documents = new();
        MoveToolPane anchorables = new();
        ILayoutGroup pane = tools ? anchorables : documents;
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent selected = CreateContent(tools);
        pane.InsertChildAt(0, selected);
        pane.InsertChildAt(1, CreateContent(tools));

        ((ILayoutPane)pane).MoveChild(0, 1);
        ((ILayoutPane)pane).MoveChild(1, 1);

        Assert.Same(selected, selector.SelectedContent);
        Assert.Equal(new[] { (0, 1, tools ? 1 : 0) }, tools ? anchorables.Moves : documents.Moves);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Changed_Observer_Can_Select_A_Different_Item_Immediately(bool tools)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent first = CreateContent(tools);
        LayoutContent second = CreateContent(tools);
        LayoutContent third = CreateContent(tools);
        pane.InsertChildAt(0, first);
        pane.InsertChildAt(1, second);
        pane.InsertChildAt(2, third);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        first.IsActive = true;
        bool redirected = false;
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "SelectedContentIndex" || redirected || !ReferenceEquals(selector.SelectedContent, second)) return;
            redirected = true;
            selector.SelectedContentIndex = 2;
            Assert.Same(third, selector.SelectedContent);
            Assert.True(third.IsSelected);
            Assert.False(second.IsSelected);
        };

        selector.SelectedContentIndex = 1;

        Assert.True(redirected);
        Assert.Same(third, selector.SelectedContent);
        Assert.Same(first, root.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Selection_Observer_Can_Explicitly_Activate_The_New_Item(bool tools)
    {
        ILayoutGroup pane = CreatePane(tools);
        ILayoutContentSelector selector = (ILayoutContentSelector)pane;
        LayoutContent first = CreateContent(tools);
        LayoutContent second = CreateContent(tools);
        pane.InsertChildAt(0, first);
        pane.InsertChildAt(1, second);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        first.IsActive = true;
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "SelectedContent" && ReferenceEquals(selector.SelectedContent, second)) second.IsActive = true;
        };

        selector.SelectedContentIndex = 1;

        Assert.Same(second, root.ActiveContent);
        Assert.True(second.IsActive);
    }

    private sealed class MoveDocumentPane : LayoutDocumentPane
    {
        internal List<(int, int, int)> Moves { get; } = new();
        protected override void ChildMoved(int oldIndex, int newIndex)
        {
            Moves.Add((oldIndex, newIndex, SelectedContentIndex));
            base.ChildMoved(oldIndex, newIndex);
        }
    }

    private sealed class MoveToolPane : LayoutAnchorablePane
    {
        internal List<(int, int, int)> Moves { get; } = new();
        protected override void ChildMoved(int oldIndex, int newIndex)
        {
            Moves.Add((oldIndex, newIndex, SelectedContentIndex));
            base.ChildMoved(oldIndex, newIndex);
        }
    }

    private static ILayoutGroup CreatePane(bool tools) => tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
    private static LayoutContent CreateContent(bool tools) => tools ? new LayoutAnchorable() : new LayoutDocument();
}
