using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Tests for global docking validation focusing on docking group rules.
/// Updated global docking rules:
/// 1) Non-grouped dockables can dock globally anywhere (regardless of target content)
/// 2) Grouped dockables can dock globally only into a target dock with the same group
/// 3) Group mismatch (different groups, or grouped -> non-grouped) is rejected
/// </summary>
public class GlobalDockingValidationTests
{
    private Factory CreateFactory()
    {
        return new Factory();
    }

    private Tool CreateTool(string id, string title, string? dockGroup = null, IDockable? owner = null)
    {
        var tool = new Tool
        {
            Id = id,
            Title = title,
            DockGroup = dockGroup,
            CanDrag = true,
            CanDrop = true,
            Owner = owner
        };
        
        // If no owner specified, create a simple owner dock
        if (owner == null)
        {
            var ownerDock = new ToolDock
            {
                Id = $"Owner_{id}",
                Title = $"Owner of {title}",
                DockGroup = dockGroup,
                CanDrop = true,
                VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable> { tool }
            };
            tool.Owner = ownerDock;
        }
        
        return tool;
    }

    private ProportionalDock CreateGlobalTarget(string id, string? dockGroup = null)
    {
        return new ProportionalDock
        {
            Id = id,
            Title = $"Global Target {id}",
            DockGroup = dockGroup,
            CanDrop = true,
            VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>()
        };
    }

    private ToolDock CreateLocalTarget(string id, string? dockGroup = null)
    {
        return new ToolDock
        {
            Id = id,
            Title = $"Local Target {id}",
            DockGroup = dockGroup,
            CanDrop = true,
            VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>()
        };
    }

    private DockManager CreateDockManager()
    {
        return new DockManager(new DockService());
    }

    #region Global Docking Rules Tests Through DockManager

    [AvaloniaFact]
    public void GlobalDocking_Rule1_NonGroupedSource_NonGroupedTarget_ShouldAllow()
    {
        // Rule 1: Non-grouped dockables should be able to dock into global targets (non-grouped)
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: null);
        var target = CreateGlobalTarget("Target", dockGroup: null);

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result, "Non-grouped source should be able to dock into non-grouped global target");
    }

    [AvaloniaFact]
    public void GlobalDocking_Rule1_NonGroupedSource_GroupedTarget_ShouldAllow()
    {
        // Rule 1: Non-grouped dockables can dock globally anywhere (regardless of target content)
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: null);
        var target = CreateGlobalTarget("Target", dockGroup: "GroupA");

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result, "Non-grouped source should be able to dock globally anywhere");
    }

    [AvaloniaFact]
    public void GlobalDocking_Rule2_GroupedSource_SameGroupTarget_ShouldAllow()
    {
        // Rule 2: Grouped dockables can dock globally only into same-group targets
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: "GroupA");
        var target = CreateGlobalTarget("Target", dockGroup: "GroupA");

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result, "Grouped source with same group should be able to dock globally");
    }

    [AvaloniaFact]
    public void GlobalDocking_Rule3_GroupedSource_DifferentGroupTarget_ShouldReject()
    {
        // Rule 3: Different groups are incompatible for global docking
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: "GroupA");
        var target = CreateGlobalTarget("Target", dockGroup: "GroupB");

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result, "Grouped source should NOT be able to dock into different-group global target");
    }

    [AvaloniaFact]
    public void GlobalDocking_Rule2_GroupedSource_NonGroupedTarget_ShouldReject()
    {
        // Rule 2/3: Grouped dockables cannot dock into non-grouped global targets
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: "GroupA");
        var target = CreateGlobalTarget("Target", dockGroup: null);

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result, "Grouped source should NOT be able to dock into non-grouped global target");
    }

    [AvaloniaFact]
    public void GlobalDocking_DefaultEnabledPerDock_ShouldAllow()
    {
        // Test that EnableGlobalDocking is enabled by default on dock instances
        var target = CreateGlobalTarget("Target", dockGroup: null);
        
        // Verify default value is true
        Assert.True(target.EnableGlobalDocking, "EnableGlobalDocking should be enabled by default");
    }

    [AvaloniaFact]
    public void GlobalDocking_DisabledPerDock_ShouldReject()
    {
        // Test that global docking can be disabled per dock
        var source = CreateTool("Source", "Source Tool", dockGroup: null);
        var target = CreateGlobalTarget("Target", dockGroup: null);
        
        // Disable global docking on the target dock
        target.EnableGlobalDocking = false;

        var dockManager = CreateDockManager();
        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        // Manager now honors EnableGlobalDocking inheritance; per-dock disable should reject.
        Assert.False(result, "Global docking disabled on target dock must be rejected");
    }

    #endregion

    #region Comprehensive Rule Testing

    [AvaloniaTheory]
    [InlineData(null, null, true)]          // Rule 1: Non-grouped can dock globally anywhere
    [InlineData(null, "GroupA", true)]      // Rule 1: Non-grouped can dock globally anywhere
    [InlineData("GroupA", "GroupA", true)]  // Rule 2: Grouped source can dock globally into same group
    [InlineData("GroupA", "GroupB", false)] // Rule 3: Different groups are incompatible
    [InlineData("GroupA", null, false)]     // Rule 3: Grouped -> non-grouped is rejected
    public void GlobalDocking_AllRuleScenarios_ThroughDockManager(string? sourceGroup, string? targetGroup, bool expected)
    {
        var dockManager = CreateDockManager();
        var source = CreateTool("Source", "Source Tool", dockGroup: sourceGroup);
        var target = CreateGlobalTarget("Target", dockGroup: targetGroup);

        var result = dockManager.ValidateTool(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Integration Tests

    [AvaloniaFact]
    public void Integration_GlobalDockingRules_WorkThroughDockManager()
    {
        var dockManager = CreateDockManager();

        // Test Rule 1: Non-grouped can dock into non-grouped global target
        var nonGroupedTool = CreateTool("NonGrouped", "Non-grouped Tool", null);
        var nonGroupedGlobalTarget = CreateGlobalTarget("NonGroupedGlobal", null);
        
        var result1 = dockManager.ValidateTool(nonGroupedTool, nonGroupedGlobalTarget, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result1, "Non-grouped tool should be able to dock into non-grouped global target");

        // Test Rule 1: Non-grouped CAN dock into grouped global target (can dock anywhere)
        var groupedGlobalTarget = CreateGlobalTarget("GroupedGlobal", "GroupA");
        
        var result2 = dockManager.ValidateTool(nonGroupedTool, groupedGlobalTarget, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result2, "Non-grouped tool should be able to dock globally anywhere");

        // Test Rule 2: Grouped CAN dock globally into same-group target
        var groupedTool = CreateTool("Grouped", "Grouped Tool", "GroupA");
        var sameGroupGlobalTarget = CreateGlobalTarget("SameGroupGlobal", "GroupA");
        
        var result3 = dockManager.ValidateTool(groupedTool, sameGroupGlobalTarget, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result3, "Grouped tool should be able to dock globally into same-group target");

        // Test Rule 3: Grouped sources are blocked from docking into different-group global targets
        var differentGroupGlobalTarget = CreateGlobalTarget("DifferentGroupGlobal", "GroupB");
        
        var result4 = dockManager.ValidateTool(groupedTool, differentGroupGlobalTarget, DragAction.Move, DockOperation.Fill, false);
        Assert.False(result4, "Grouped tool should NOT be able to dock globally into different-group target");
    }

    [AvaloniaFact]
    public void Integration_DockGroupValidator_GlobalValidation()
    {
        // Test the DockGroupValidator.ValidateGlobalDocking directly to ensure our logic is sound
        
        // Rule 1: Non-grouped sources can dock globally anywhere
        var result1 = DockGroupValidator.ValidateGlobalDocking(
            CreateTool("Source", "Tool", null),
            CreateGlobalTarget("Target", null));
        Assert.True(result1);

        // Rule 1: Non-grouped sources can dock globally anywhere (even into grouped targets)
        var result2 = DockGroupValidator.ValidateGlobalDocking(
            CreateTool("Source", "Tool", null),
            CreateGlobalTarget("Target", "GroupA"));
        Assert.True(result2);

        // Rule 2: Grouped sources can dock globally only into same-group targets
        var result3 = DockGroupValidator.ValidateGlobalDocking(
            CreateTool("Source", "Tool", "GroupA"),
            CreateGlobalTarget("Target", "GroupA"));
        Assert.True(result3);

        // Rule 3: Grouped sources cannot dock globally into different-group targets
        var result4 = DockGroupValidator.ValidateGlobalDocking(
            CreateTool("Source", "Tool", "GroupA"),
            CreateGlobalTarget("Target", "GroupB"));
        Assert.False(result4);

        // Rule 3: Grouped sources cannot dock globally into non-grouped targets
        var result5 = DockGroupValidator.ValidateGlobalDocking(
            CreateTool("Source", "Tool", "GroupA"),
            CreateGlobalTarget("Target", null));
        Assert.False(result5);
    }

    #endregion
}
