using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class ToolDockGripModeInheritanceTests
{
    [Fact]
    public void SplitToolDockable_Inherits_GripMode_From_TargetDock()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var toolDock = new ToolDock
        {
            GripMode = GripMode.AutoHide,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var tool1 = new Tool { Id = "Tool1", Title = "Tool1" };
        var tool2 = new Tool { Id = "Tool2", Title = "Tool2" };

        toolDock.VisibleDockables.Add(tool1);
        toolDock.VisibleDockables.Add(tool2);
        toolDock.ActiveDockable = tool1;
        root.VisibleDockables.Add(toolDock);

        factory.InitLayout(root);

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(tool2, toolDock, DragAction.Move, DockOperation.Right, bExecute: true);

        Assert.True(executed);
        var newDock = tool2.Owner as IToolDock;
        Assert.NotNull(newDock);
        Assert.NotSame(toolDock, newDock);
        Assert.Equal(GripMode.AutoHide, newDock!.GripMode);
    }
}
