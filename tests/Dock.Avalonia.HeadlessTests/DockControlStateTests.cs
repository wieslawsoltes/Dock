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
    public void GlobalDockOperationSelector_UsesGlobal_WhenNoLocalAdorner()
    {
        var service = new GlobalDockingService();

        var useGlobal = service.ShouldUseGlobalOperation(
            hasLocalAdorner: false,
            localOperation: DockOperation.Fill,
            globalOperation: DockOperation.Right);

        Assert.True(useGlobal);
    }

    [AvaloniaFact]
    public void GlobalDockOperationSelector_UsesLocal_WhenLocalOperationIsExplicit()
    {
        var service = new GlobalDockingService();

        var useGlobal = service.ShouldUseGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Top,
            globalOperation: DockOperation.Right);

        Assert.False(useGlobal);
    }

    [AvaloniaFact]
    public void GlobalDockOperationSelector_UsesGlobal_WhenLocalOperationIsWindow()
    {
        var service = new GlobalDockingService();

        var useGlobal = service.ShouldUseGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Window,
            globalOperation: DockOperation.Bottom);

        Assert.True(useGlobal);
    }

    [AvaloniaFact]
    public void GlobalDockOperationSelector_UsesLocal_WhenGlobalOperationIsNone()
    {
        var service = new GlobalDockingService();

        var useGlobal = service.ShouldUseGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Window,
            globalOperation: DockOperation.None);

        Assert.False(useGlobal);
    }

    [AvaloniaFact]
    public void GlobalDockingProportionService_TryApply_UpdatesOwnerProportionAndCollapsedProportion()
    {
        var service = new GlobalDockingService();
        var sourceDock = new DocumentDock();
        var sourceDocument = new Document { Owner = sourceDock };
        var sourceRoot = new RootDock();
        var targetRoot = new RootDock();

        var apply = service.TryApplyGlobalDockingProportion(sourceDocument, sourceRoot, targetRoot, proportion: 0.5);

        Assert.True(apply);
        Assert.Equal(0.5, sourceDock.Proportion, 3);
        Assert.Equal(0.5, sourceDock.CollapsedProportion, 3);
    }

    [AvaloniaFact]
    public void GlobalDockingProportionService_TryApply_ReturnsFalse_WhenSourceRootMissing()
    {
        var service = new GlobalDockingService();
        var sourceDocument = new Document
        {
            Id = "SourceDocument",
            Title = "SourceDocument",
            Owner = new DocumentDock()
        };
        var targetRoot = new RootDock();

        var apply = service.TryApplyGlobalDockingProportion(sourceDocument, sourceRoot: null, targetRoot, proportion: 0.5);

        Assert.False(apply);
    }

    [AvaloniaFact]
    public void GlobalDockingProportionService_TryApply_ReturnsFalse_WhenTargetRootMissing()
    {
        var service = new GlobalDockingService();
        var sourceDock = new DocumentDock();
        var sourceDocument = new Document { Owner = sourceDock };
        var sourceRoot = new RootDock();

        var apply = service.TryApplyGlobalDockingProportion(sourceDocument, sourceRoot, targetRoot: null, proportion: 0.5);

        Assert.False(apply);
        Assert.True(double.IsNaN(sourceDock.Proportion));
        Assert.True(double.IsNaN(sourceDock.CollapsedProportion));
    }

    [AvaloniaFact]
    public void GlobalDockingProportionService_TryApply_ReturnsFalse_WhenSourceOwnerMissing()
    {
        var service = new GlobalDockingService();
        var sourceDocument = new Document();
        var sourceRoot = new RootDock();
        var targetRoot = new RootDock();

        var apply = service.TryApplyGlobalDockingProportion(sourceDocument, sourceRoot, targetRoot, proportion: 0.5);

        Assert.False(apply);
    }

    [AvaloniaFact]
    public void GlobalDockTargetResolver_UsesOwnerDock_FromDropDataContextDockable()
    {
        var service = new GlobalDockingService();
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
        var targetDock = service.ResolveGlobalTargetDock(dropControl);

        Assert.Same(documentDock, targetDock);
    }

    [AvaloniaFact]
    public void GlobalDockTargetResolver_UsesDropDock_FromDropDataContextDock()
    {
        var service = new GlobalDockingService();
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
        var targetDock = service.ResolveGlobalTargetDock(dropControl);

        Assert.Same(documentDock, targetDock);
    }
}
