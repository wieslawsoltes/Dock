using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactorySelectionPolicyTests
{
    [AvaloniaTheory]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    [InlineData(1, 0)]
    public void Default_Removal_Policy_Preserves_Previous_Or_First_Selection(int count, int removedIndex)
    {
        Factory factory = new();
        AvaloniaList<IDockable> documents = new();
        for (int index = 0; index < count; index++) documents.Add(new Document());
        DocumentDock pane = new() { VisibleDockables = documents, IsCollapsable = false };
        RootDock root = Initialize(factory, pane);
        IDockable removed = documents[removedIndex];
        pane.ActiveDockable = removed;

        factory.RemoveDockable(removed, false);

        IDockable? expected = count == 1 ? null : documents[removedIndex > 0 ? removedIndex - 1 : 0];
        Assert.Same(expected, pane.ActiveDockable);
        Assert.Same(expected, root.FocusedDockable);
        Assert.False(((Document)removed).IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Removal_Policy_Receives_The_Removed_Item_And_Can_Choose_Or_Clear_Selection(bool clear)
    {
        PolicyFactory factory = new();
        Document first = new();
        Document removed = new();
        Document chosen = new();
        DocumentDock pane = new() { VisibleDockables = new AvaloniaList<IDockable>(first, removed, chosen), IsCollapsable = false };
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = removed;
        factory.Replacement = clear ? null : chosen;

        factory.RemoveDockable(removed, false);

        Assert.Same(pane, factory.RemovalOwner);
        Assert.Same(removed, factory.Removed);
        Assert.Equal(1, factory.RemovedIndex);
        Assert.DoesNotContain(removed, factory.ChildrenAtRemoval!);
        Assert.Same(clear ? null : chosen, pane.ActiveDockable);
        Assert.Same(clear ? null : chosen, root.FocusedDockable);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Focus_Policy_Can_Keep_Focus_While_Native_Selection_Still_Synchronizes(bool focus)
    {
        PolicyFactory factory = new();
        Document first = new();
        Document selected = new();
        DocumentDock pane = new() { VisibleDockables = new AvaloniaList<IDockable>(first, selected), IsCollapsable = false };
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = first;
        factory.FocusSelection = focus;

        pane.ActiveDockable = selected;

        Assert.Same(pane, factory.FocusOwner);
        Assert.Same(selected, factory.FocusTarget);
        Assert.Same(focus ? selected : first, root.FocusedDockable);
        Assert.True(selected.IsSelected);
        Assert.False(first.IsSelected);
    }

    private static RootDock Initialize(Factory factory, DocumentDock pane)
    {
        RootDock root = new()
        {
            VisibleDockables = new AvaloniaList<IDockable>(pane),
            ActiveDockable = pane,
            DefaultDockable = pane,
            IsFocusableRoot = true,
            IsCollapsable = false
        };
        factory.InitLayout(root);
        return root;
    }

    private sealed class PolicyFactory : Factory
    {
        internal bool FocusSelection { get; set; } = true;
        internal IDockable? Replacement { get; set; }
        internal IDock? RemovalOwner { get; private set; }
        internal IDockable? Removed { get; private set; }
        internal int RemovedIndex { get; private set; }
        internal IList<IDockable>? ChildrenAtRemoval { get; private set; }
        internal IDock? FocusOwner { get; private set; }
        internal IDockable? FocusTarget { get; private set; }

        protected override IDockable? GetActiveDockableAfterRemoval(IDock dock, IDockable removedDockable, int removedIndex)
        {
            RemovalOwner = dock;
            Removed = removedDockable;
            RemovedIndex = removedIndex;
            ChildrenAtRemoval = new List<IDockable>(dock.VisibleDockables!);
            return Replacement;
        }

        protected override bool ShouldFocusActiveDockable(IDockable dockable, IDock owner)
        {
            FocusOwner = owner;
            FocusTarget = dockable;
            return FocusSelection;
        }
    }
}
