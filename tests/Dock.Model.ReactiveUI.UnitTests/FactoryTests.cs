using System.Collections.ObjectModel;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class FactoryTests
{
    [Fact]
    public void TestFactory_Ctor()
    {
        var actual = new TestFactory();
        Assert.NotNull(actual);
    }

    [Fact]
    public void CreateList_Creates_ReactiveList_Empty()
    {
        var factory = new TestFactory();
        var actual = factory.CreateList<IDockable>();
        Assert.NotNull(actual);
        Assert.IsType<ObservableCollection<IDockable>>(actual);
        Assert.Empty(actual);
    }

    [Fact]
    public void CreateRootDock_Creates_RootDock_Type()
    {
        var factory = new TestFactory();
        var actual = factory.CreateRootDock();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [Fact]
    public void CreateProportionalDock_Creates_ProportionalDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDock();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDock>(actual);
    }

    [Fact]
    public void CreateDockDock_Creates_DockDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockDock();
        Assert.NotNull(actual);
        Assert.IsType<DockDock>(actual);
        Assert.True(actual.LastChildFill);
    }

    [Fact]
    public void CreateProportionalDockSplitter_Creates_ProportionalDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDockSplitter>(actual);
        Assert.True(actual.CanResize);
        Assert.False(actual.ResizePreview);
    }

    [Fact]
    public void CreateToolDock_Creates_ToolDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateToolDock();
        Assert.NotNull(actual);
        Assert.IsType<ToolDock>(actual);
    }

    [Fact]
    public void Tool_Default_Sizes_Are_NaN()
    {
        var tool = new Tool();
        Assert.True(double.IsNaN(tool.MinWidth));
        Assert.True(double.IsNaN(tool.MaxWidth));
        Assert.True(double.IsNaN(tool.MinHeight));
        Assert.True(double.IsNaN(tool.MaxHeight));
    }

    [Fact]
    public void CreateDocumentDock_Creates_DocumentDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocumentDock();
        Assert.NotNull(actual);
        Assert.IsType<DocumentDock>(actual);
    }

    [Fact]
    public void CreateDockWindow_Creates_DockWindow()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockWindow();
        Assert.NotNull(actual);
        Assert.IsType<DockWindow>(actual);
    }

    [Fact]
    public void CreateLayout_Creates_RootDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateLayout();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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
}

public class TestFactory : Factory
{
}
