using System.Collections.Generic;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockManagerCapabilityPolicyTests
{
    private static (DockManager Manager, RootDock Root, ToolDock SourceDock, ToolDock TargetDock, Tool SourceTool) CreateToolScenario()
    {
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable>()
        };

        var sourceDock = new ToolDock
        {
            Id = "SourceDock",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var targetDock = new ToolDock
        {
            Id = "TargetDock",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var sourceTool = new Tool
        {
            Id = "SourceTool",
            Owner = sourceDock
        };

        var targetTool = new Tool
        {
            Id = "TargetTool",
            Owner = targetDock
        };

        sourceDock.VisibleDockables!.Add(sourceTool);
        targetDock.VisibleDockables!.Add(targetTool);
        sourceDock.ActiveDockable = sourceTool;
        targetDock.ActiveDockable = targetTool;
        root.VisibleDockables!.Add(sourceDock);
        root.VisibleDockables!.Add(targetDock);
        root.ActiveDockable = sourceDock;

        var manager = new DockManager(new DockService());
        return (manager, root, sourceDock, targetDock, sourceTool);
    }

    [Fact]
    public void ValidateTool_Blocks_When_Root_Policy_Disables_Drag()
    {
        var (manager, root, _, targetDock, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = false
        };

        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.NotNull(manager.LastCapabilityEvaluation);
        Assert.Equal(DockCapability.Drag, manager.LastCapabilityEvaluation!.Capability);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, manager.LastCapabilityEvaluation.EffectiveSource);
    }

    [Fact]
    public void ValidateTool_Dock_Policy_Overrides_Root_Policy()
    {
        var (manager, root, sourceDock, targetDock, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = false
        };
        sourceDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = true
        };

        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }

    [Fact]
    public void ValidateTool_Dockable_Override_Overrides_Dock_Policy()
    {
        var (manager, root, sourceDock, targetDock, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = true
        };
        sourceDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = true
        };
        sourceTool.DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanDrag = false
        };

        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.NotNull(manager.LastCapabilityEvaluation);
        Assert.Equal(DockCapability.Drag, manager.LastCapabilityEvaluation!.Capability);
        Assert.Equal(DockCapabilityValueSource.DockableOverride, manager.LastCapabilityEvaluation.EffectiveSource);
    }

    [Fact]
    public void ValidateTool_Blocks_When_Target_Dock_Policy_Disables_Drop()
    {
        var (manager, _, _, targetDock, sourceTool) = CreateToolScenario();
        targetDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrop = false
        };

        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.NotNull(manager.LastCapabilityEvaluation);
        Assert.Equal(DockCapability.Drop, manager.LastCapabilityEvaluation!.Capability);
        Assert.Equal(DockCapabilityValueSource.DockPolicy, manager.LastCapabilityEvaluation.EffectiveSource);
    }

    [Fact]
    public void ValidateTool_Blocks_Document_Target_When_DockAsDocument_Disabled_By_Policy()
    {
        var (manager, root, _, _, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDockAsDocument = false
        };

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        root.VisibleDockables!.Add(documentDock);

        var result = manager.ValidateTool(sourceTool, documentDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.NotNull(manager.LastCapabilityEvaluation);
        Assert.Equal(DockCapability.DockAsDocument, manager.LastCapabilityEvaluation!.Capability);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, manager.LastCapabilityEvaluation.EffectiveSource);
    }

    [Fact]
    public void ValidateTool_Clears_Diagnostics_When_Failure_Is_Not_Capability_Based()
    {
        var (manager, root, _, targetDock, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = false
        };

        _ = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);
        Assert.NotNull(manager.LastCapabilityEvaluation);

        manager.IsDockingEnabled = false;
        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }

    [Fact]
    public void ValidateDockable_Preserves_First_Capability_Diagnostic_For_Composite_Docks()
    {
        var manager = new DockManager(new DockService());
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable>()
        };

        var source = new ProportionalDock
        {
            Id = "Source",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var childFail = new ToolDock
        {
            Id = "ChildFail",
            Owner = source,
            VisibleDockables = new List<IDockable>(),
            DockCapabilityPolicy = new DockCapabilityPolicy
            {
                CanDrag = false
            }
        };

        var childPass = new ToolDock
        {
            Id = "ChildPass",
            Owner = source,
            VisibleDockables = new List<IDockable>()
        };

        var failTool = new Tool
        {
            Id = "FailTool",
            Owner = childFail
        };

        var passTool = new Tool
        {
            Id = "PassTool",
            Owner = childPass
        };

        childFail.VisibleDockables!.Add(failTool);
        childPass.VisibleDockables!.Add(passTool);
        childFail.ActiveDockable = failTool;
        childPass.ActiveDockable = passTool;

        // Order matters: validation iterates from the end.
        // Put failing dock last so later successful validation could wipe diagnostics.
        source.VisibleDockables!.Add(childPass);
        source.VisibleDockables!.Add(childFail);
        source.ActiveDockable = childPass;

        var targetDock = new ToolDock
        {
            Id = "Target",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        var targetTool = new Tool
        {
            Id = "TargetTool",
            Owner = targetDock
        };
        targetDock.VisibleDockables!.Add(targetTool);
        targetDock.ActiveDockable = targetTool;

        root.VisibleDockables!.Add(source);
        root.VisibleDockables!.Add(targetDock);
        root.ActiveDockable = source;

        var result = manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.NotNull(manager.LastCapabilityEvaluation);
        Assert.Equal(DockCapability.Drag, manager.LastCapabilityEvaluation!.Capability);
        Assert.Equal(DockCapabilityValueSource.DockPolicy, manager.LastCapabilityEvaluation.EffectiveSource);
    }

    [Fact]
    public void ValidateTool_Ignores_DockAsDocument_Policy_For_NonDocument_Target()
    {
        var (manager, root, _, targetDock, sourceTool) = CreateToolScenario();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDockAsDocument = false
        };

        var result = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }

    [Fact]
    public void ValidateDockable_CompositeDock_ReturnsFalse_When_No_Visible_Dockables()
    {
        var manager = new DockManager(new DockService());
        var source = new ProportionalDock
        {
            VisibleDockables = new List<IDockable>()
        };
        var targetDock = new ToolDock
        {
            VisibleDockables = new List<IDockable>()
        };

        var result = manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }

    [Fact]
    public void ValidateDockable_CompositeDock_With_NonDock_Children_ReturnsTrue()
    {
        var manager = new DockManager(new DockService());
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable>()
        };

        var source = new ProportionalDock
        {
            Id = "Source",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        var sourceTool = new Tool
        {
            Id = "SourceTool",
            Owner = source
        };
        source.VisibleDockables!.Add(sourceTool);

        var targetDock = new ToolDock
        {
            Id = "TargetDock",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        var targetTool = new Tool
        {
            Id = "TargetTool",
            Owner = targetDock
        };
        targetDock.VisibleDockables!.Add(targetTool);

        root.VisibleDockables!.Add(source);
        root.VisibleDockables!.Add(targetDock);

        var result = manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }

    [Fact]
    public void ValidateDockable_CompositeDock_Failure_Without_Capability_Diagnostic_Keeps_Diagnostic_Null()
    {
        var manager = new DockManager(new DockService());
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable>()
        };

        var source = new ProportionalDock
        {
            Id = "Source",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var childFail = new ToolDock
        {
            Id = "ChildFail",
            Owner = source,
            VisibleDockables = new List<IDockable>(),
            AllowedDockOperations = DockOperationMask.None
        };
        var failTool = new Tool
        {
            Id = "FailTool",
            Owner = childFail
        };
        childFail.VisibleDockables!.Add(failTool);
        childFail.ActiveDockable = failTool;
        source.VisibleDockables!.Add(childFail);

        var targetDock = new ToolDock
        {
            Id = "TargetDock",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        var targetTool = new Tool
        {
            Id = "TargetTool",
            Owner = targetDock
        };
        targetDock.VisibleDockables!.Add(targetTool);
        targetDock.ActiveDockable = targetTool;

        root.VisibleDockables!.Add(source);
        root.VisibleDockables!.Add(targetDock);
        root.ActiveDockable = source;

        var result = manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        Assert.Null(manager.LastCapabilityEvaluation);
    }
}
