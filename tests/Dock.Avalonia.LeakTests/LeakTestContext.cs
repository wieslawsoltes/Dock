using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.LeakTests;

internal sealed class LeakContext
{
    private LeakContext(
        Factory factory,
        RootDock root,
        ToolDock toolDock,
        DocumentDock documentDock,
        Tool tool,
        Document document,
        StackDock stackDock,
        ProportionalDock proportionalDock,
        GridDock gridDock,
        UniformGridDock uniformGridDock,
        WrapDock wrapDock,
        DockDock dockDock,
        SplitViewDock splitViewDock,
        ManagedWindowDock managedWindowDock)
    {
        Factory = factory;
        Root = root;
        ToolDock = toolDock;
        DocumentDock = documentDock;
        Tool = tool;
        Document = document;
        StackDock = stackDock;
        ProportionalDock = proportionalDock;
        GridDock = gridDock;
        UniformGridDock = uniformGridDock;
        WrapDock = wrapDock;
        DockDock = dockDock;
        SplitViewDock = splitViewDock;
        ManagedWindowDock = managedWindowDock;
    }

    public Factory Factory { get; }

    public RootDock Root { get; }

    public ToolDock ToolDock { get; }

    public DocumentDock DocumentDock { get; }

    public Tool Tool { get; }

    public Document Document { get; }

    public StackDock StackDock { get; }

    public ProportionalDock ProportionalDock { get; }

    public GridDock GridDock { get; }

    public UniformGridDock UniformGridDock { get; }

    public WrapDock WrapDock { get; }

    public DockDock DockDock { get; }

    public SplitViewDock SplitViewDock { get; }

    public ManagedWindowDock ManagedWindowDock { get; }

    public static LeakContext Create(Factory? factoryOverride = null)
    {
        var factory = factoryOverride ?? new Factory();

        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (Tool)factory.CreateTool();
        tool.Factory = factory;

        var document = (Document)factory.CreateDocument();
        document.Factory = factory;

        var toolDock = (ToolDock)factory.CreateToolDock();
        toolDock.Factory = factory;
        toolDock.VisibleDockables = factory.CreateList<IDockable>(tool);
        toolDock.ActiveDockable = tool;
        tool.Owner = toolDock;

        var documentDock = (DocumentDock)factory.CreateDocumentDock();
        documentDock.Factory = factory;
        documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
        documentDock.ActiveDockable = document;
        document.Owner = documentDock;

        var stackDock = (StackDock)factory.CreateStackDock();
        stackDock.Factory = factory;
        stackDock.VisibleDockables = factory.CreateList<IDockable>();

        var proportionalDock = (ProportionalDock)factory.CreateProportionalDock();
        proportionalDock.Factory = factory;
        proportionalDock.VisibleDockables = factory.CreateList<IDockable>();

        var gridDock = (GridDock)factory.CreateGridDock();
        gridDock.Factory = factory;
        gridDock.VisibleDockables = factory.CreateList<IDockable>();

        var uniformGridDock = (UniformGridDock)factory.CreateUniformGridDock();
        uniformGridDock.Factory = factory;
        uniformGridDock.VisibleDockables = factory.CreateList<IDockable>();

        var wrapDock = (WrapDock)factory.CreateWrapDock();
        wrapDock.Factory = factory;
        wrapDock.VisibleDockables = factory.CreateList<IDockable>();

        var dockDock = (DockDock)factory.CreateDockDock();
        dockDock.Factory = factory;
        dockDock.VisibleDockables = factory.CreateList<IDockable>();

        var splitViewDock = (SplitViewDock)factory.CreateSplitViewDock();
        splitViewDock.Factory = factory;
        splitViewDock.VisibleDockables = factory.CreateList<IDockable>();

        root.VisibleDockables.Add(toolDock);
        root.VisibleDockables.Add(documentDock);
        root.VisibleDockables.Add(stackDock);
        root.VisibleDockables.Add(proportionalDock);
        root.VisibleDockables.Add(gridDock);
        root.VisibleDockables.Add(uniformGridDock);
        root.VisibleDockables.Add(wrapDock);
        root.VisibleDockables.Add(dockDock);
        root.VisibleDockables.Add(splitViewDock);

        root.PinnedDock = toolDock;
        root.LeftPinnedDockables?.Add(tool);

        var managedWindowDock = new ManagedWindowDock
        {
            Factory = factory
        };

        return new LeakContext(
            factory,
            root,
            toolDock,
            documentDock,
            tool,
            document,
            stackDock,
            proportionalDock,
            gridDock,
            uniformGridDock,
            wrapDock,
            dockDock,
            splitViewDock,
            managedWindowDock);
    }
}
