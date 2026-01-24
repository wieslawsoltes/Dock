using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockManagerDockingRestrictionsTests
{
    [Fact]
    public void Docking_Disabled_Blocks_Validation()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var toolDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var tool1 = new Tool { Id = "Tool1", Title = "Tool1" };
        var tool2 = new Tool { Id = "Tool2", Title = "Tool2" };

        toolDock.VisibleDockables.Add(tool1);
        toolDock.VisibleDockables.Add(tool2);
        toolDock.ActiveDockable = tool1;
        root.VisibleDockables.Add(toolDock);

        factory.InitLayout(root);

        var manager = new DockManager(new DockService())
        {
            IsDockingEnabled = false
        };

        var canDock = manager.ValidateTool(tool2, toolDock, DragAction.Move, DockOperation.Right, bExecute: false);

        Assert.False(canDock);
    }

    [Fact]
    public void Docking_Restrictions_Block_Source_Operation()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var toolDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var tool1 = new Tool
        {
            Id = "Tool1",
            Title = "Tool1",
            AllowedDockOperations = DockOperationMask.Fill
        };

        var tool2 = new Tool { Id = "Tool2", Title = "Tool2" };

        toolDock.VisibleDockables.Add(tool1);
        toolDock.VisibleDockables.Add(tool2);
        toolDock.ActiveDockable = tool1;
        root.VisibleDockables.Add(toolDock);

        factory.InitLayout(root);

        var manager = new DockManager(new DockService());
        var canDock = manager.ValidateTool(tool1, toolDock, DragAction.Move, DockOperation.Right, bExecute: false);

        Assert.False(canDock);
    }

    [Fact]
    public void Docking_Restrictions_Block_Target_Operation()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var toolDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            AllowedDropOperations = DockOperationMask.Fill
        };

        var tool1 = new Tool { Id = "Tool1", Title = "Tool1" };
        var tool2 = new Tool { Id = "Tool2", Title = "Tool2" };

        toolDock.VisibleDockables.Add(tool1);
        toolDock.VisibleDockables.Add(tool2);
        toolDock.ActiveDockable = tool1;
        root.VisibleDockables.Add(toolDock);

        factory.InitLayout(root);

        var manager = new DockManager(new DockService());
        var canDock = manager.ValidateTool(tool2, toolDock, DragAction.Move, DockOperation.Right, bExecute: false);

        Assert.False(canDock);
    }
}
