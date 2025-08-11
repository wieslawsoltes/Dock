using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

/// <summary>
/// Tests for global docking property inheritance through the dock hierarchy.
/// </summary>
public class GlobalDockingInheritanceTests
{
    private ProportionalDock CreateParentDock(bool enableGlobalDocking = true, string? dockGroup = null)
    {
        return new ProportionalDock
        {
            Id = "ParentDock",
            Title = "Parent Dock",
            EnableGlobalDocking = enableGlobalDocking,
            DockGroup = dockGroup,
            VisibleDockables = new AvaloniaList<IDockable>()
        };
    }

    private ToolDock CreateChildDock(IDockable? owner, bool enableGlobalDocking = true, string? dockGroup = null)
    {
        return new ToolDock
        {
            Id = "ChildDock",
            Title = "Child Dock",
            Owner = owner,
            EnableGlobalDocking = enableGlobalDocking,
            DockGroup = dockGroup,
            VisibleDockables = new AvaloniaList<IDockable>()
        };
    }

    private Tool CreateTool(IDockable? owner, string? dockGroup = null)
    {
        return new Tool
        {
            Id = "Tool",
            Title = "Test Tool",
            Owner = owner,
            DockGroup = dockGroup
        };
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_NoAncestors_ShouldReturnTrue()
    {
        var dock = CreateParentDock(enableGlobalDocking: true);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(dock);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_SelfDisabled_ShouldReturnFalse()
    {
        var dock = CreateParentDock(enableGlobalDocking: false);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(dock);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_ParentDisabled_ShouldReturnFalse()
    {
        var parent = CreateParentDock(enableGlobalDocking: false);
        var child = CreateChildDock(parent, enableGlobalDocking: true);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(child);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_ChildDisabled_ShouldReturnFalse()
    {
        var parent = CreateParentDock(enableGlobalDocking: true);
        var child = CreateChildDock(parent, enableGlobalDocking: false);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(child);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_BothEnabled_ShouldReturnTrue()
    {
        var parent = CreateParentDock(enableGlobalDocking: true);
        var child = CreateChildDock(parent, enableGlobalDocking: true);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(child);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_ThreeLevel_MiddleDisabled_ShouldReturnFalse()
    {
        var grandparent = CreateParentDock(enableGlobalDocking: true);
        var parent = CreateChildDock(grandparent, enableGlobalDocking: false);
        var child = CreateChildDock(parent, enableGlobalDocking: true);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(child);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_NonDockAncestor_ShouldSkip()
    {
        var parentDock = CreateParentDock(enableGlobalDocking: false);
        var tool = CreateTool(parentDock);
        var childDock = CreateChildDock(tool, enableGlobalDocking: true);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(childDock);

        // Should find the parent dock that has EnableGlobalDocking = false
        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetEffectiveEnableGlobalDocking_NonDockTarget_ShouldWalkUpToFindDock()
    {
        var parentDock = CreateParentDock(enableGlobalDocking: false);
        var tool = CreateTool(parentDock);

        var result = DockInheritanceHelper.GetEffectiveEnableGlobalDocking(tool);

        // Should walk up through the tool to find the parent dock that disables global docking
        Assert.False(result);
    }
}
