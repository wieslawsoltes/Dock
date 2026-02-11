using Avalonia.Headless.XUnit;
using Avalonia.Collections;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
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
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);
    }

    [AvaloniaFact]
    public void DockingState_Transitions_Cover_Docked_Pinned_Document_Floating_And_Hidden()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(root, documentDock);

        var tool = factory.CreateTool();
        factory.AddDockable(toolDock, tool);

        var document = factory.CreateDocument();
        factory.AddDockable(documentDock, document);

        Assert.Equal(DockingWindowState.Docked, tool.DockingState);
        Assert.Equal(DockingWindowState.Document, document.DockingState);

        factory.DockAsDocument(tool);
        Assert.Equal(DockingWindowState.Document, tool.DockingState);

        factory.MoveDockable(documentDock, toolDock, tool, null);
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);

        factory.PinDockable(tool);
        Assert.Equal(DockingWindowState.Pinned, tool.DockingState);

        factory.UnpinDockable(tool);
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);

        factory.FloatDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.HideDockable(tool);
        Assert.Equal(
            DockingWindowState.Docked | DockingWindowState.Floating | DockingWindowState.Hidden,
            tool.DockingState);

        factory.RestoreDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.PinDockable(tool);
        Assert.Equal(DockingWindowState.Pinned | DockingWindowState.Floating, tool.DockingState);

        factory.UnpinDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.FloatDockable(document);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);

        factory.HideDockable(document);
        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            document.DockingState);

        factory.RestoreDockable(document);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);
    }


    [AvaloniaFact]
    public void DockingState_HiddenContainer_Propagates_To_Descendants()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        var document = factory.CreateDocument();

        factory.AddDockable(root, documentDock);
        factory.AddDockable(documentDock, document);

        factory.HideDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Hidden, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Hidden, document.DockingState);

        factory.RestoreDockable(documentDock);

        Assert.Equal(DockingWindowState.Document, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document, document.DockingState);

        factory.FloatDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);

        factory.HideDockable(documentDock);

        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            documentDock.DockingState);
        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            document.DockingState);

        factory.RestoreDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);
    }

    [AvaloniaFact]
    public void DockingWindowStateMixin_Synchronizes_Layout_To_ViewModel()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);

        var tool = (Tool)factory.CreateTool();
        factory.AddDockable(toolDock, tool);

        Assert.True(tool.IsOpen);
        Assert.False(tool.IsSelected);
        Assert.False(tool.IsActive);

        toolDock.ActiveDockable = tool;
        factory.SetFocusedDockable(toolDock, tool);

        Assert.True(tool.IsSelected);
        Assert.True(tool.IsActive);

        factory.HideDockable(tool);

        Assert.False(tool.IsOpen);
        Assert.True(tool.DockingState.HasFlag(DockingWindowState.Hidden));
        Assert.False(tool.IsSelected);
        Assert.False(tool.IsActive);

        factory.RestoreDockable(tool);

        Assert.True(tool.IsOpen);
        Assert.False(tool.DockingState.HasFlag(DockingWindowState.Hidden));
    }

    [AvaloniaFact]
    public void DockingWindowStateMixin_HiddenAncestor_Resets_ChildOpenSelectionAndActiveFlags()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, documentDock);

        var document = (Document)factory.CreateDocument();
        factory.AddDockable(documentDock, document);
        documentDock.ActiveDockable = document;
        factory.SetFocusedDockable(documentDock, document);

        Assert.True(document.IsOpen);
        Assert.True(document.IsSelected);
        Assert.True(document.IsActive);

        factory.HideDockable(documentDock);

        Assert.True(document.DockingState.HasFlag(DockingWindowState.Hidden));
        Assert.False(document.IsOpen);
        Assert.False(document.IsSelected);
        Assert.False(document.IsActive);
    }

    [AvaloniaFact]
    public void DockingWindowStateMixin_Synchronizes_ViewModel_To_Layout()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(root, documentDock);

        var tool = (Tool)factory.CreateTool();
        factory.AddDockable(toolDock, tool);

        tool.DockingState = DockingWindowState.Document;

        Assert.Same(documentDock, tool.Owner);
        Assert.True(tool.DockingState.HasFlag(DockingWindowState.Document));

        tool.IsOpen = false;

        Assert.Contains(tool, root.HiddenDockables);
        Assert.False(tool.IsOpen);

        tool.IsOpen = true;

        Assert.DoesNotContain(tool, root.HiddenDockables);
        Assert.True(tool.IsOpen);

        tool.IsSelected = true;
        Assert.Same(tool, documentDock.ActiveDockable);

        tool.IsActive = true;
        Assert.Same(tool, root.FocusedDockable);
        Assert.True(tool.IsActive);
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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
