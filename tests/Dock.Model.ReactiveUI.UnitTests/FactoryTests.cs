using System.Collections.ObjectModel;
using Dock.Model.Controls;
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

    [Fact]
    public void OnActiveDockableChanged_Includes_Root_And_Window_Context()
    {
        var factory = new TestFactory();
        var context = CreateDockableContext(factory);

        IRootDock? raisedRoot = null;
        IDockWindow? raisedWindow = null;

        factory.ActiveDockableChanged += (_, args) =>
        {
            raisedRoot = args.RootDock;
            raisedWindow = args.Window;
        };

        factory.OnActiveDockableChanged(context.Dockable);

        Assert.Same(context.Root, raisedRoot);
        Assert.Same(context.Window, raisedWindow);
    }

    [Fact]
    public void GlobalDockTrackingChanged_Tracks_Window_And_Dockable()
    {
        var factory = new TestFactory();
        var context = CreateDockableContext(factory);
        var eventRaised = false;
        GlobalDockTrackingState? current = null;

        factory.GlobalDockTrackingChanged += (_, args) =>
        {
            eventRaised = true;
            current = args.Current;
        };

        factory.OnWindowActivated(context.Window);

        Assert.True(eventRaised);
        Assert.NotNull(current);
        Assert.Same(context.Dockable, current!.Dockable);
        Assert.Same(context.Root, current.RootDock);
        Assert.Same(context.Window, current.Window);
        Assert.Same(context.Dockable, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void OnWindowRemoved_Clears_Current_Global_Tracking()
    {
        var factory = new TestFactory();
        var context = CreateDockableContext(factory);

        factory.OnWindowActivated(context.Window);
        Assert.Same(context.Window, factory.CurrentDockWindow);

        factory.OnWindowRemoved(context.Window);

        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
        Assert.Null(factory.CurrentHostWindow);
    }

    [Fact]
    public void ActiveDockableChanged_From_Different_Root_Does_Not_Override_Current_Global_State()
    {
        var factory = new TestFactory();
        var first = CreateDockableContext(factory);
        var second = CreateDockableContext(factory);

        factory.OnWindowActivated(first.Window);
        factory.OnActiveDockableChanged(second.Dockable);

        Assert.Same(first.Dockable, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void ActiveDockableChanged_Null_Does_Not_Clear_Current_Global_State()
    {
        var factory = new TestFactory();
        var context = CreateDockableContext(factory);

        factory.OnWindowActivated(context.Window);
        factory.OnActiveDockableChanged(null);

        Assert.Same(context.Dockable, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void FocusedDockableChanged_From_Different_Root_Does_Not_Override_Current_Global_State()
    {
        var factory = new TestFactory();
        var first = CreateDockableContext(factory);
        var second = CreateDockableContext(factory);

        factory.OnWindowActivated(first.Window);
        factory.OnFocusedDockableChanged(second.Dockable);

        Assert.Same(first.Dockable, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void SetActiveDockable_From_Different_Root_Does_Not_Override_Current_Global_State()
    {
        var factory = new TestFactory();
        var first = CreateDockableContext(factory);
        var second = CreateDockableContext(factory);

        factory.OnWindowActivated(first.Window);
        factory.SetActiveDockable(second.Dockable);

        Assert.Same(first.Dockable, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    private static (IRootDock Root, IDockWindow Window, IDock Dock, IDockable Dockable) CreateDockableContext(TestFactory factory)
    {
        var root = factory.CreateRootDock();
        var window = factory.CreateDockWindow();
        var dock = factory.CreateDocumentDock();
        var dockable = factory.CreateDocument();

        dock.VisibleDockables = factory.CreateList<IDockable>(dockable);
        dock.Owner = root;
        dock.ActiveDockable = dockable;
        dock.FocusedDockable = dockable;
        dockable.Owner = dock;
        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        root.ActiveDockable = dock;
        root.FocusedDockable = dockable;
        window.Layout = root;
        root.Window = window;

        return (root, window, dock, dockable);
    }
}

public class TestFactory : Factory
{
}
