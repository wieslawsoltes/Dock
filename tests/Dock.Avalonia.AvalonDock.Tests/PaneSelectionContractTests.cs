using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class PaneSelectionContractTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Pane_Selection_Publishes_The_Nested_Clear_Before_Selecting_The_Requested_Child(bool tools, bool rooted)
    {
        Fixture fixture = new(tools, rooted);

        fixture.Selector.SelectedContentIndex = 1;

        Assert.Equal(new[]
        {
            "pane:changing:SelectedContentIndex:0:True:False",
            "pane:changing:SelectedContent:0:True:False",
            "first:changing:0:True:False",
            "pane:changing:SelectedContentIndex:0:False:False",
            "pane:changing:SelectedContent:0:False:False",
            "pane:changed:SelectedContentIndex:-1:False:False",
            "pane:changed:SelectedContent:-1:False:False",
            "first:event:-1:False:False",
            "first:changed:-1:False:False",
            "second:changing:1:False:False",
            "second:event:1:False:True",
            "second:changed:1:False:True",
            "pane:changed:SelectedContentIndex:1:False:True",
            "pane:changed:SelectedContent:1:False:True"
        }, fixture.Trace);
        Assert.Same(fixture.Second.DockModel, fixture.Native.ActiveDockable);
        GC.KeepAlive(fixture.Root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Content_Selection_Writes_Its_Flag_Before_The_Parent_Changing_Events(bool tools, bool rooted)
    {
        Fixture fixture = new(tools, rooted);

        fixture.Second.IsSelected = true;

        Assert.Equal(new[]
        {
            "second:changing:0:True:False",
            "pane:changing:SelectedContentIndex:0:True:True",
            "pane:changing:SelectedContent:0:True:True",
            "first:changing:0:True:True",
            "pane:changing:SelectedContentIndex:0:False:True",
            "pane:changing:SelectedContent:0:False:True",
            "pane:changed:SelectedContentIndex:-1:False:True",
            "pane:changed:SelectedContent:-1:False:True",
            "first:event:-1:False:True",
            "first:changed:-1:False:True",
            "pane:changed:SelectedContentIndex:1:False:True",
            "pane:changed:SelectedContent:1:False:True",
            "second:event:1:False:True",
            "second:changed:1:False:True"
        }, fixture.Trace);
        GC.KeepAlive(fixture.Root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Throwing_Parent_Changing_Observer_Leaves_The_New_Content_Flag_Without_Its_Event(bool tools, bool rooted)
    {
        Fixture fixture = new(tools, rooted);
        fixture.Pane.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == "SelectedContentIndex") throw new ProbeException();
        };

        Assert.Throws<ProbeException>(() => fixture.Second.IsSelected = true);

        Assert.True(fixture.First.IsSelected);
        Assert.True(fixture.Second.IsSelected);
        Assert.Equal(0, fixture.Selector.SelectedContentIndex);
        Assert.Equal(new[]
        {
            "second:changing:0:True:False",
            "pane:changing:SelectedContentIndex:0:True:True"
        }, fixture.Trace);
        GC.KeepAlive(fixture.Root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Clearing_A_Selected_Flag_Clears_The_Parent_Even_When_Another_Child_Occupies_Its_Index(bool tools, bool rooted)
    {
        Fixture fixture = new(tools, rooted);
        bool fail = true;
        fixture.Pane.PropertyChanging += (_, args) =>
        {
            if (fail && args.PropertyName == "SelectedContentIndex") throw new ProbeException();
        };
        Assert.Throws<ProbeException>(() => fixture.Second.IsSelected = true);
        fail = false;
        fixture.Trace.Clear();

        fixture.Second.IsSelected = false;

        Assert.Equal(-1, fixture.Selector.SelectedContentIndex);
        Assert.False(fixture.First.IsSelected);
        Assert.False(fixture.Second.IsSelected);
        Assert.Null(fixture.Native.ActiveDockable);
        GC.KeepAlive(fixture.Root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void A_Changed_Observer_Sees_The_Nested_Clear_But_The_Outer_Request_Continues(bool tools, bool rooted)
    {
        Fixture fixture = new(tools, rooted);
        LayoutContent third = tools ? new LayoutAnchorable() : new LayoutDocument();
        fixture.Pane.InsertChildAt(2, third);
        bool redirected = false;
        fixture.Pane.PropertyChanged += (_, args) =>
        {
            if (redirected || args.PropertyName != "SelectedContentIndex") return;
            redirected = true;
            Assert.Equal(-1, fixture.Selector.SelectedContentIndex);
            fixture.Selector.SelectedContentIndex = 2;
        };

        fixture.Selector.SelectedContentIndex = 1;

        Assert.True(redirected);
        Assert.Same(fixture.Second, fixture.Selector.SelectedContent);
        Assert.True(fixture.Second.IsSelected);
        Assert.True(third.IsSelected);
        Assert.False(fixture.First.IsSelected);
        Assert.Same(fixture.Second.DockModel, fixture.Native.ActiveDockable);
        GC.KeepAlive(fixture.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Repeated_Content_Selection_Does_Not_Repair_A_Divergent_Parent(bool tools)
    {
        Fixture fixture = new(tools, false);
        fixture.Pane.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == "SelectedContentIndex") throw new ProbeException();
        };
        Assert.Throws<ProbeException>(() => fixture.Second.IsSelected = true);
        fixture.Trace.Clear();

        fixture.Second.IsSelected = true;

        Assert.Empty(fixture.Trace);
        Assert.Same(fixture.First, fixture.Selector.SelectedContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Invalid_Indices_Normalize_Before_Equality_And_Notification(bool tools)
    {
        Fixture fixture = new(tools, false);
        fixture.Selector.SelectedContentIndex = -1;
        fixture.Trace.Clear();

        fixture.Selector.SelectedContentIndex = -2;
        fixture.Selector.SelectedContentIndex = fixture.Pane.ChildrenCount;

        Assert.Empty(fixture.Trace);
        Assert.Null(fixture.Selector.SelectedContent);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Suppressing_The_Child_Move_Base_Hook_Retains_The_Collection_Phases_Index(bool tools, bool callBase)
    {
        MoveDocumentPane documents = new() { CallBase = callBase };
        MoveToolPane anchorables = new() { CallBase = callBase };
        ILayoutGroup pane = tools ? anchorables : documents;
        LayoutContent selected = tools ? new LayoutAnchorable() : new LayoutDocument();
        pane.InsertChildAt(0, selected);
        pane.InsertChildAt(1, tools ? new LayoutAnchorable() : new LayoutDocument());

        ((ILayoutPane)pane).MoveChild(0, 1);

        Assert.Equal(tools || callBase ? 1 : 0, ((ILayoutContentSelector)pane).SelectedContentIndex);
        Assert.Equal(tools ? 1 : 0, tools ? anchorables.IndexBeforeBase : documents.IndexBeforeBase);
        Assert.True(selected.IsSelected);
    }

    [AvaloniaFact]
    public void A_Rooted_Document_Fallback_Still_Activates_Its_Chosen_Child_After_The_Pane_Is_Detached()
    {
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        pane.SelectedContentIndex = -1;
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "SelectedContentIndex" && pane.SelectedContentIndex == 0)
                root.RootPanel.Children.Remove(pane);
        };

        pane.Children.Add(new LayoutDocument());

        Assert.Null(pane.Root);
        Assert.True(document.IsActive);
        Assert.True(document.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void An_Unchanged_Document_Index_Projects_Its_New_Occupant_Without_Rewriting_Public_Flags(bool rooted, bool selectContent)
    {
        LayoutDocument original = new();
        LayoutDocumentPane pane = new(original);
        LayoutRoot? root = rooted ? new LayoutRoot { RootPanel = new LayoutPanel(pane) } : null;
        LayoutDocument inserted = new();
        pane.Children.Insert(0, inserted);
        bool selectedBeforeAssignment = inserted.IsSelected;
        int changes = 0;
        pane.PropertyChanged += (_, args) => { if (args.PropertyName is "SelectedContentIndex" or "SelectedContent") changes++; };

        if (selectContent) inserted.IsSelected = true;
        else pane.SelectedContentIndex = 0;

        Assert.Equal(0, changes);
        Assert.Same(inserted, pane.SelectedContent);
        Assert.Same(inserted.DockModel, ((IDock)pane.DockModel).ActiveDockable);
        Assert.Equal(selectContent || selectedBeforeAssignment, inserted.IsSelected);
        Assert.True(original.IsSelected);
        GC.KeepAlive(root);
    }

    private sealed class MoveDocumentPane : LayoutDocumentPane
    {
        internal bool CallBase { get; init; }
        internal int IndexBeforeBase { get; private set; }
        protected override void ChildMoved(int oldIndex, int newIndex)
        {
            IndexBeforeBase = SelectedContentIndex;
            if (CallBase) base.ChildMoved(oldIndex, newIndex);
        }
    }

    private sealed class MoveToolPane : LayoutAnchorablePane
    {
        internal bool CallBase { get; init; }
        internal int IndexBeforeBase { get; private set; }
        protected override void ChildMoved(int oldIndex, int newIndex)
        {
            IndexBeforeBase = SelectedContentIndex;
            if (CallBase) base.ChildMoved(oldIndex, newIndex);
        }
    }

    private sealed class Fixture
    {
        internal Fixture(bool tools, bool rooted)
        {
            First = tools ? new LayoutAnchorable() : new LayoutDocument();
            Second = tools ? new LayoutAnchorable() : new LayoutDocument();
            Pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
            Pane.InsertChildAt(0, First);
            Pane.InsertChildAt(1, Second);
            if (rooted) Root = new LayoutRoot { RootPanel = new LayoutPanel((ILayoutPanelElement)Pane) };
            Selector = (ILayoutContentSelector)Pane;
            Native = (IDock)((LayoutElement)Pane).DockModel;
            Pane.PropertyChanging += (_, args) =>
            {
                if (args.PropertyName is "SelectedContentIndex" or "SelectedContent") Record("pane:changing:" + args.PropertyName);
            };
            Pane.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is "SelectedContentIndex" or "SelectedContent") Record("pane:changed:" + args.PropertyName);
            };
            Observe(First, "first");
            Observe(Second, "second");
        }

        internal ILayoutGroup Pane { get; }
        internal ILayoutContentSelector Selector { get; }
        internal IDock Native { get; }
        internal LayoutContent First { get; }
        internal LayoutContent Second { get; }
        internal LayoutRoot? Root { get; }
        internal List<string> Trace { get; } = new();

        private void Observe(LayoutContent content, string name)
        {
            content.PropertyChanging += (_, args) => { if (args.PropertyName == "IsSelected") Record(name + ":changing"); };
            content.PropertyChanged += (_, args) => { if (args.PropertyName == "IsSelected") Record(name + ":changed"); };
            content.IsSelectedChanged += (_, _) => Record(name + ":event");
        }

        private void Record(string phase) => Trace.Add($"{phase}:{Selector.SelectedContentIndex}:{First.IsSelected}:{Second.IsSelected}");
    }

    private sealed class ProbeException : Exception;
}
