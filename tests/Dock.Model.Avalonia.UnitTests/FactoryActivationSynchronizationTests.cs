using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactoryActivationSynchronizationTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Activation_Policy_Preserves_Flags_While_Focus_Selection_And_Events_Continue(bool tools, bool synchronize)
    {
        PolicyFactory factory = new();
        IDockable first = tools ? new Tool() : new Document();
        IDockable second = tools ? new Tool() : new Document();
        IDock pane = tools ? new ToolDock() : new DocumentDock();
        pane.VisibleDockables = new AvaloniaList<IDockable>(first, second);
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = first;
        factory.SynchronizeActivation = synchronize;
        List<IDockable?> events = new();
        factory.FocusedDockableChanged += (_, args) => events.Add(args.Dockable);

        pane.ActiveDockable = second;

        Assert.Same(second, root.FocusedDockable);
        Assert.Same(second, pane.ActiveDockable);
        Assert.Contains(second, events);
        IDockingWindowState firstState = (IDockingWindowState)first;
        IDockingWindowState secondState = (IDockingWindowState)second;
        Assert.Equal(!synchronize, firstState.IsActive);
        Assert.Equal(synchronize, secondState.IsActive);
        Assert.False(firstState.IsSelected);
        Assert.True(secondState.IsSelected);
        Assert.True(firstState.IsOpen);
        Assert.True(secondState.IsOpen);
        Assert.Equal(tools ? DockingWindowState.Docked : DockingWindowState.Document, secondState.DockingState);

        factory.SynchronizeActivation = true;
        pane.ActiveDockable = second;
        Assert.False(firstState.IsActive);
        Assert.True(secondState.IsActive);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Assigning_The_Active_Flag_Still_Requests_Native_Focus(bool tools)
    {
        PolicyFactory factory = new();
        IDockable first = tools ? new Tool() : new Document();
        IDockable second = tools ? new Tool() : new Document();
        IDock pane = tools ? new ToolDock() : new DocumentDock();
        pane.VisibleDockables = new AvaloniaList<IDockable>(first, second);
        RootDock root = Initialize(factory, pane);
        pane.ActiveDockable = first;
        factory.SynchronizeActivation = false;

        ((IDockingWindowState)second).IsActive = true;

        Assert.Same(second, pane.ActiveDockable);
        Assert.Same(second, root.FocusedDockable);
        Assert.True(((IDockingWindowState)first).IsActive);
        Assert.True(((IDockingWindowState)second).IsActive);
        Assert.False(((IDockingWindowState)first).IsSelected);
        Assert.True(((IDockingWindowState)second).IsSelected);
    }

    [AvaloniaFact]
    public void Hidden_Content_Can_Retain_External_Activation_Without_Opening_Or_Focusing_It()
    {
        PolicyFactory factory = new() { SynchronizeActivation = false };
        Tool hidden = new() { IsActive = true };
        RootDock root = new() { HiddenDockables = new AvaloniaList<IDockable>(hidden), IsFocusableRoot = true };

        factory.InitLayout(root);

        Assert.True(hidden.IsActive);
        Assert.False(hidden.IsOpen);
        Assert.False(hidden.IsSelected);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Hidden, hidden.DockingState);
        Assert.Null(root.FocusedDockable);
        Assert.Same(hidden, Assert.Single(root.HiddenDockables));

        factory.SynchronizeActivation = true;
        factory.InitDockable(hidden, root);
        Assert.False(hidden.IsActive);
        Assert.False(hidden.IsOpen);
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
        internal bool SynchronizeActivation { get; set; } = true;
        protected override bool ShouldSynchronizeDockingWindowActivation(IDockable dockable) => SynchronizeActivation;
    }
}
