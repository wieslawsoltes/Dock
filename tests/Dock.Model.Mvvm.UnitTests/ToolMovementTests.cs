using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Xunit;

namespace Dock.Model.Mvvm.UnitTests;

public class ToolMovementTests
{
    [Fact]
    public void Moving_Tool_To_AutoHide_Dock_Expands_Target()
    {
        var factory = new Factory();

        var left = new ToolDock { Alignment = Alignment.Left, IsExpanded = true, AutoHide = false };
        var right = new ToolDock { Alignment = Alignment.Right, IsExpanded = false, AutoHide = true };
        left.VisibleDockables = factory.CreateList<IDockable>();
        right.VisibleDockables = factory.CreateList<IDockable>();

        var leftTool = new Tool { Id = "left" };
        var rightTool = new Tool { Id = "right" };

        factory.AddDockable(left, leftTool);
        factory.AddDockable(right, rightTool);

        var layout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(left, right)
        };

        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>(layout) };
        factory.InitLayout(root);

        factory.MoveDockable(left, right, leftTool, null);

        Assert.True(right.IsExpanded);
        Assert.Contains(leftTool, right.VisibleDockables!);
        Assert.Null(root.HiddenDockables);
    }
}

