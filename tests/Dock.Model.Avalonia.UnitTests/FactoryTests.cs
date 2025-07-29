using Avalonia.Headless.XUnit;
using Avalonia.Collections;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactoryTests
{
    [AvaloniaFact]
    public void TestFactory_Ctor()
    {
        var actual = new TestFactory();
        Assert.NotNull(actual);
    }

    [AvaloniaFact]
    public void CreateList_Creates_AvaloniaList_Empty()
    {
        var factory = new TestFactory();
        var actual = factory.CreateList<IDockable>();
        Assert.NotNull(actual);
        Assert.IsType<AvaloniaList<IDockable>>(actual);
        Assert.Empty(actual);
    }

    [AvaloniaFact]
    public void CreateRootDock_Creates_RootDock_Type()
    {
        var factory = new TestFactory();
        var actual = factory.CreateRootDock();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDock_Creates_ProportionalDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDock();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockDock_Creates_DockDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockDock();
        Assert.NotNull(actual);
        Assert.IsType<DockDock>(actual);
        Assert.True(actual.LastChildFill);
    }

    [AvaloniaFact]
    public void CreateStackDock_Creates_StackDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateStackDock();
        Assert.NotNull(actual);
        Assert.IsType<StackDock>(actual);
    }

    [AvaloniaFact]
    public void CreateGridDock_Creates_GridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDock();
        Assert.NotNull(actual);
        Assert.IsType<GridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateWrapDock_Creates_WrapDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateWrapDock();
        Assert.NotNull(actual);
        Assert.IsType<WrapDock>(actual);
    }

    [AvaloniaFact]
    public void CreateUniformGridDock_Creates_UniformGridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateUniformGridDock();
        Assert.NotNull(actual);
        Assert.IsType<UniformGridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDockSplitter_Creates_ProportionalDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDockSplitter>(actual);
        Assert.True(actual.CanResize);
        Assert.False(actual.ResizePreview);
    }

    [AvaloniaFact]
    public void CreateGridDockSplitter_Creates_GridDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<GridDockSplitter>(actual);
    }

    [AvaloniaFact]
    public void CreateToolDock_Creates_ToolDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateToolDock();
        Assert.NotNull(actual);
        Assert.IsType<ToolDock>(actual);
    }

    [AvaloniaFact]
    public void Tool_Default_Sizes_Are_NaN()
    {
        var tool = new Tool();
        Assert.True(double.IsNaN(tool.MinWidth));
        Assert.True(double.IsNaN(tool.MaxWidth));
        Assert.True(double.IsNaN(tool.MinHeight));
        Assert.True(double.IsNaN(tool.MaxHeight));
    }

    [AvaloniaFact]
    public void CreateDocumentDock_Creates_DocumentDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocumentDock();
        Assert.NotNull(actual);
        Assert.IsType<DocumentDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockWindow_Creates_DockWindow()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockWindow();
        Assert.NotNull(actual);
        Assert.IsType<DockWindow>(actual);
    }

    [AvaloniaFact]
    public void CreateLayout_Creates_RootDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateLayout();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [AvaloniaFact]
    public void OnWindowActivated_Raises_Event()
    {
        var factory = new TestFactory();
        var window = factory.CreateDockWindow();
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.OnWindowActivated(window);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }

    [AvaloniaFact]
    public void OnDockableActivated_Raises_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.OnDockableActivated(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void SetActiveDockable_Triggers_DockableActivated_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var dock = factory.CreateDocumentDock();
        
        // Set up the dock hierarchy
        dock.VisibleDockables = factory.CreateList<IDockable>(dockable);
        dockable.Owner = dock; // Set the owner relationship
        
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.SetActiveDockable(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void OnWindowDeactivated_Raises_Event()
    {
        var factory = new TestFactory();
        var window = factory.CreateDockWindow();
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowDeactivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.OnWindowDeactivated(window);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }

    [AvaloniaFact]
    public void OnDockableDeactivated_Raises_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableDeactivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.OnDockableDeactivated(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void ActivateWindow_Triggers_WindowActivated_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var root = factory.CreateRootDock();
        var window = factory.CreateDockWindow();
        
        // Set up the window hierarchy
        root.VisibleDockables = factory.CreateList<IDockable>(dockable);
        root.ActiveDockable = dockable;
        dockable.Owner = root; // Set the owner relationship
        window.Layout = root;
        root.Window = window;
        
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.ActivateWindow(dockable);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }
}

public class TestFactory : Factory
{
}
