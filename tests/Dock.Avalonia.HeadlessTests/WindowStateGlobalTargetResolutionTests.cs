using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class WindowStateGlobalTargetResolutionTests
{
    private static (Factory factory, RootDock root, Document sourceDocument, Document targetDocument) CreateScenario()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, mainLayout);

        var sourceDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var targetDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var unrelatedToolDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        factory.AddDockable(mainLayout, sourceDock);
        factory.AddDockable(mainLayout, targetDock);
        factory.AddDockable(mainLayout, unrelatedToolDock);

        var sourceDocument = new Document
        {
            Id = "SourceDocument",
            Title = "SourceDocument"
        };
        var targetDocument = new Document
        {
            Id = "TargetDocument",
            Title = "TargetDocument"
        };
        var unrelatedTool = new Tool
        {
            Id = "Tool1",
            Title = "Tool1"
        };

        factory.AddDockable(sourceDock, sourceDocument);
        sourceDock.ActiveDockable = sourceDocument;

        factory.AddDockable(targetDock, targetDocument);
        targetDock.ActiveDockable = targetDocument;

        factory.AddDockable(unrelatedToolDock, unrelatedTool);
        unrelatedToolDock.ActiveDockable = unrelatedTool;

        mainLayout.ActiveDockable = unrelatedToolDock;
        root.ActiveDockable = mainLayout;
        // Keep source focus on the dragged document while active pane points elsewhere.
        root.FocusedDockable = sourceDocument;

        return (factory, root, sourceDocument, targetDocument);
    }

    private static bool InvokeValidateGlobal(DockManagerState state, Control dropControl, Visual relativeTo)
    {
        var dropControlProperty = typeof(DockManagerState)
            .GetProperty("DropControl", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(dropControlProperty);
        dropControlProperty!.SetValue(state, dropControl);

        var validateGlobal = state.GetType()
            .GetMethod("ValidateGlobal", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(validateGlobal);

        var result = validateGlobal!.Invoke(
            state,
            new object[] { new Point(20, 20), DockOperation.Fill, DragAction.Move, relativeTo });

        return Assert.IsType<bool>(result);
    }

    [AvaloniaFact]
    public void HostWindowState_ValidateGlobal_UsesDropContextTarget_WithoutDockControlAncestor()
    {
        var (factory, root, sourceDocument, targetDocument) = CreateScenario();

        var hostWindow = new HostWindow
        {
            Window = new DockWindow
            {
                Factory = factory,
                Layout = root
            }
        };
        var manager = new DockManager(new DockService());
        var state = new HostWindowState(manager, hostWindow);

        // Intentionally not hosted in DockControl to ensure target resolution uses drop context.
        var dropControl = new Border { DataContext = targetDocument };
        var visualRoot = new Window
        {
            Width = 300,
            Height = 200,
            Content = dropControl
        };

        visualRoot.Show();
        try
        {
            var targetDock = DockManagerState.ResolveGlobalTargetDock(dropControl);
            Assert.NotNull(targetDock);
            Assert.Same(targetDocument.Owner, targetDock);
            Assert.NotNull(dropControl.GetVisualRoot());
            Assert.NotNull(root.FocusedDockable);
            Assert.Same(sourceDocument, root.FocusedDockable);
            Assert.True(DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock!));
            Assert.True(DockGroupValidator.ValidateGlobalDocking(root.FocusedDockable!, targetDock!));
            Assert.True(manager.ValidateDockable(root.FocusedDockable!, targetDock!, DragAction.Move, DockOperation.Fill, bExecute: false));

            var valid = InvokeValidateGlobal(state, dropControl, dropControl);
            Assert.True(valid);
        }
        finally
        {
            visualRoot.Close();
        }
    }

    [AvaloniaFact]
    public void ManagedHostWindowState_ValidateGlobal_UsesDropContextTarget_WithoutDockControlAncestor()
    {
        var (factory, root, sourceDocument, targetDocument) = CreateScenario();

        var hostWindow = new ManagedHostWindow
        {
            Window = new DockWindow
            {
                Factory = factory,
                Layout = root
            }
        };
        var manager = new DockManager(new DockService());
        var state = new ManagedHostWindowState(manager, hostWindow);

        // Intentionally not hosted in DockControl to ensure target resolution uses drop context.
        var dropControl = new Border { DataContext = targetDocument };
        var visualRoot = new Window
        {
            Width = 300,
            Height = 200,
            Content = dropControl
        };

        visualRoot.Show();
        try
        {
            var targetDock = DockManagerState.ResolveGlobalTargetDock(dropControl);
            Assert.NotNull(targetDock);
            Assert.Same(targetDocument.Owner, targetDock);
            Assert.NotNull(dropControl.GetVisualRoot());
            Assert.NotNull(root.FocusedDockable);
            Assert.Same(sourceDocument, root.FocusedDockable);
            Assert.True(DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock!));
            Assert.True(DockGroupValidator.ValidateGlobalDocking(root.FocusedDockable!, targetDock!));
            Assert.True(manager.ValidateDockable(root.FocusedDockable!, targetDock!, DragAction.Move, DockOperation.Fill, bExecute: false));

            var valid = InvokeValidateGlobal(state, dropControl, dropControl);
            Assert.True(valid);
        }
        finally
        {
            visualRoot.Close();
        }
    }
}
