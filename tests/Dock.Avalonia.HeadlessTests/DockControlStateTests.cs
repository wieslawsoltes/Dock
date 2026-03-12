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
using Dock.Settings;
using System.Reflection;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlStateTests
{
    private sealed class RecordingFactory : Factory
    {
        public int FloatCount { get; private set; }
        public IDockable? LastFloatedDockable { get; private set; }

        public override void FloatDockable(IDockable dockable)
        {
            FloatCount++;
            LastFloatedDockable = dockable;
        }
    }

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
    public void Process_Released_Floats_WhenResolvedDropTargetHasDifferentDockGroup()
    {
        var factory = new RecordingFactory();
        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var sourceDock = factory.CreateDocumentDock();
        sourceDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, sourceDock);

        var targetDock = factory.CreateDocumentDock();
        targetDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, targetDock);

        var sourceDocument = factory.CreateDocument();
        sourceDocument.DockGroup = "Documents";
        factory.AddDockable(sourceDock, sourceDocument);

        var targetDocument = factory.CreateDocument();
        targetDocument.DockGroup = "Widgets";
        factory.AddDockable(targetDock, targetDocument);

        var dockControl = new DockControl
        {
            Layout = root
        };
        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = dockControl
        };

        try
        {
            window.Show();
            window.UpdateLayout();
            dockControl.ApplyTemplate();

            var state = CreateState(new DockManager(new DockService()));
            var dragControl = new Control
            {
                DataContext = sourceDocument
            };
            var dropControl = new Border
            {
                DataContext = targetDocument
            };
            dropControl.SetValue(DockProperties.IsDropEnabledProperty, true);

            var contextField = typeof(DockControlState)
                .GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var context = contextField.GetValue(state)!;
            context.GetType().GetProperty("DragControl")!.SetValue(context, dragControl);
            context.GetType().GetProperty("DoDragDrop")!.SetValue(context, true);
            context.GetType().GetProperty("TargetPoint")!.SetValue(context, new Point(5, 5));
            context.GetType().GetProperty("TargetDockControl")!.SetValue(context, dockControl);
            context.GetType().GetProperty("ResolvedOperation")!.SetValue(context, DockOperation.Fill);
            context.GetType().GetProperty("UseGlobalOperation")!.SetValue(context, false);
            context.GetType().GetProperty("HasResolvedOperation")!.SetValue(context, true);

            var dropControlProperty = typeof(DockManagerState)
                .GetProperty("DropControl", BindingFlags.Instance | BindingFlags.NonPublic)!;
            dropControlProperty.SetValue(state, dropControl);

            state.Process(new Point(5, 5), default, EventType.Released, DragAction.Move, dockControl, new List<IDockControl> { dockControl });

            Assert.Equal(1, factory.FloatCount);
            Assert.Same(sourceDocument, factory.LastFloatedDockable);
        }
        finally
        {
            window.Close();
        }
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
    public void GlobalDockOperationSelector_UsesGlobal_WhenLocalOperationIsExplicit()
    {
        var service = new GlobalDockingService();

        var useGlobal = service.ShouldUseGlobalOperation(
            hasLocalAdorner: true,
            localOperation: DockOperation.Top,
            globalOperation: DockOperation.Right);

        Assert.True(useGlobal);
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
    public void GlobalDockTargetResolver_UsesOutermostGlobalTarget_FromDropDataContextDockable()
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

        Assert.Same(rootLayout, targetDock);
    }

    [AvaloniaFact]
    public void GlobalDockTargetResolver_UsesOutermostGlobalTarget_FromDropDataContextDock()
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

        Assert.Same(rootLayout, targetDock);
    }

    [AvaloniaFact]
    public void GlobalDockTargetResolver_UsesOutermostGlobalTarget_WhenNested()
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

        var middleColumnLayout = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(rootLayout, middleColumnLayout);

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(middleColumnLayout, documentDock);

        var document = new Document { Id = "Document1", Title = "Document 1" };
        factory.AddDockable(documentDock, document);

        var dropControl = new Border { DataContext = document };
        var targetDock = service.ResolveGlobalTargetDock(dropControl);

        Assert.Same(rootLayout, targetDock);
    }
}
