using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlStateTests
{
    private static DockControlState CreateState(DockManager manager)
    {
        return new DockControlState(manager, new DefaultDragOffsetCalculator());
    }


    [AvaloniaFact]
    public void Process_CaptureLost_Ends_Drag()
    {
        var manager = new DockManager(new DockService());
        var state = CreateState(manager);
        var dock = new DockControl();
        var docks = new List<IDockControl> { dock };

        state.Process(new Point(0,0), new Vector(), EventType.Pressed, DragAction.Move, dock, docks);
        state.Process(new Point(10,10), new Vector(), EventType.Moved, DragAction.Move, dock, docks);
        state.Process(new Point(), new Vector(), EventType.CaptureLost, DragAction.None, dock, docks);

        Assert.False(dock.IsDraggingDock);
    }

    [AvaloniaFact]
    public void Process_Released_Ends_Drag()
    {
        var manager = new DockManager(new DockService());
        var state = CreateState(manager);
        var dock = new DockControl();
        var docks = new List<IDockControl> { dock };

        state.Process(new Point(0,0), new Vector(), EventType.Pressed, DragAction.Move, dock, docks);
        state.Process(new Point(10,10), new Vector(), EventType.Moved, DragAction.Move, dock, docks);
        state.Process(new Point(), new Vector(), EventType.Released, DragAction.None, dock, docks);

        Assert.False(dock.IsDraggingDock);
    }

    [AvaloniaFact]
    public void PreferGlobalOperation_UsesGlobal_WhenNoLocalAdorner()
    {
        var useGlobal = DockControlState.PreferGlobalOperation(
            hasLocalAdorner: false,
            localOperation: DockOperation.Fill,
            globalOperation: DockOperation.Right);

        Assert.True(useGlobal);
    }

    [AvaloniaFact]
    public void PreferGlobalOperation_UsesLocal_WhenLocalOperationIsExplicit()
    {
        var useGlobal = DockControlState.PreferGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Top,
            globalOperation: DockOperation.Right);

        Assert.False(useGlobal);
    }

    [AvaloniaFact]
    public void PreferGlobalOperation_UsesGlobal_WhenLocalOperationIsWindow()
    {
        var useGlobal = DockControlState.PreferGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Window,
            globalOperation: DockOperation.Bottom);

        Assert.True(useGlobal);
    }

    [AvaloniaFact]
    public void PreferGlobalOperation_UsesLocal_WhenGlobalOperationIsNone()
    {
        var useGlobal = DockControlState.PreferGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Window,
            globalOperation: DockOperation.None);

        Assert.False(useGlobal);
    }

    [AvaloniaFact]
    public void ShouldApplyGlobalDockingProportion_ReturnsTrue_ForSameRoot()
    {
        var root = new RootDock();

        var apply = DockControlState.ShouldApplyGlobalDockingProportion(root, root);

        Assert.True(apply);
    }

    [AvaloniaFact]
    public void ShouldApplyGlobalDockingProportion_ReturnsTrue_ForDifferentRoots()
    {
        var sourceRoot = new RootDock();
        var targetRoot = new RootDock();

        var apply = DockControlState.ShouldApplyGlobalDockingProportion(sourceRoot, targetRoot);

        Assert.True(apply);
    }

    [AvaloniaFact]
    public void ShouldApplyGlobalDockingProportion_ReturnsFalse_WhenRootMissing()
    {
        var root = new RootDock();

        var applyMissingSource = DockControlState.ShouldApplyGlobalDockingProportion(null, root);
        var applyMissingTarget = DockControlState.ShouldApplyGlobalDockingProportion(root, null);

        Assert.False(applyMissingSource);
        Assert.False(applyMissingTarget);
    }

    [AvaloniaFact]
    public void ResolveGlobalTargetDock_UsesOwnerDock_FromDropDataContextDockable()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var rootLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, rootLayout);

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(rootLayout, documentDock);

        var toolDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(rootLayout, toolDock);

        var document = new Document { Id = "Document1", Title = "Document 1" };
        factory.AddDockable(documentDock, document);
        documentDock.ActiveDockable = document;

        var tool = new Tool { Id = "Tool1", Title = "Tool 1" };
        factory.AddDockable(toolDock, tool);
        toolDock.ActiveDockable = tool;

        // Simulate unrelated active pane to ensure resolution follows drop context.
        rootLayout.ActiveDockable = toolDock;
        root.ActiveDockable = rootLayout;

        var dropControl = new Border { DataContext = document };
        var targetDock = DockManagerState.ResolveGlobalTargetDock(dropControl);

        Assert.Same(documentDock, targetDock);
    }

    [AvaloniaFact]
    public void ResolveGlobalTargetDock_UsesDropDock_FromDropDataContextDock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var rootLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, rootLayout);

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(rootLayout, documentDock);

        var dropControl = new Border { DataContext = documentDock };
        var targetDock = DockManagerState.ResolveGlobalTargetDock(dropControl);

        Assert.Same(documentDock, targetDock);
    }
}
