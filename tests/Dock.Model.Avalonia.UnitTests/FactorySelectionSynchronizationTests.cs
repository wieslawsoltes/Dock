using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactorySelectionSynchronizationTests
{
    [AvaloniaTheory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Selection_Policy_Preserves_External_Flags_Without_Suppressing_Focus_Or_Events(bool tools, bool focus, bool synchronize)
    {
        PolicyFactory factory = new();
        IDockable first = tools ? new Tool() : new Document();
        IDockable second = tools ? new Tool() : new Document();
        IDock pane = tools ? new ToolDock() : new DocumentDock();
        pane.VisibleDockables = new AvaloniaList<IDockable>(first, second);
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = first;
        factory.SynchronizeSelection = synchronize;
        factory.FocusSelection = focus;
        List<IDockable?> events = new();
        factory.ActiveDockableChanged += (_, args) => events.Add(args.Dockable);

        pane.ActiveDockable = second;

        Assert.Contains(second, events);
        Assert.Same(second, pane.ActiveDockable);
        Assert.Same(focus ? second : first, root.FocusedDockable);
        IDockingWindowState firstState = (IDockingWindowState)first;
        IDockingWindowState secondState = (IDockingWindowState)second;
        Assert.Equal(!synchronize, firstState.IsSelected);
        Assert.Equal(synchronize, secondState.IsSelected);
        Assert.Equal(!focus, firstState.IsActive);
        Assert.Equal(focus, secondState.IsActive);
        Assert.True(firstState.IsOpen);
        Assert.True(secondState.IsOpen);
        Assert.Equal(tools ? DockingWindowState.Docked : DockingWindowState.Document, secondState.DockingState);

        // Reenabling the policy allows the next synchronization to repair both
        // flags, even when the pane's active item has not changed again.
        factory.SynchronizeSelection = true;
        pane.ActiveDockable = second;
        Assert.False(firstState.IsSelected);
        Assert.True(secondState.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Model_Flag_Assignment_Still_Requests_Selection_When_The_Policy_Is_Disabled(bool tools)
    {
        PolicyFactory factory = new();
        IDockable first = tools ? new Tool() : new Document();
        IDockable second = tools ? new Tool() : new Document();
        IDock pane = tools ? new ToolDock() : new DocumentDock();
        pane.VisibleDockables = new AvaloniaList<IDockable>(first, second);
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = first;
        factory.SynchronizeSelection = false;

        ((IDockingWindowState)second).IsSelected = true;

        Assert.Same(second, pane.ActiveDockable);
        Assert.Same(second, root.FocusedDockable);
        Assert.True(((IDockingWindowState)first).IsSelected);
        Assert.True(((IDockingWindowState)second).IsSelected);
    }

    private static RootDock Initialize(Factory factory, IDock pane)
    {
        RootDock root = new()
        {
            VisibleDockables = new AvaloniaList<IDockable>(pane),
            ActiveDockable = pane,
            DefaultDockable = pane,
            IsFocusableRoot = true
        };
        factory.InitLayout(root);
        return root;
    }

    private sealed class PolicyFactory : Factory
    {
        internal bool SynchronizeSelection { get; set; } = true;
        internal bool FocusSelection { get; set; } = true;

        protected override bool ShouldSynchronizeDockingWindowSelection(IDockable dockable) => SynchronizeSelection;
        protected override bool ShouldFocusActiveDockable(IDockable dockable, IDock owner) => FocusSelection;
    }
}
