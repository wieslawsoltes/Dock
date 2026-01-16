using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryPinBoundsTests
{
    [AvaloniaFact]
    public void PinDockable_UsesDockableVisibleBounds_WhenValid()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        tool.SetVisibleBounds(0, 0, 320, 240);
        toolDock.SetVisibleBounds(0, 0, 640, 480);

        factory.PinDockable(tool);

        tool.GetPinnedBounds(out _, out _, out var width, out var height);
        Assert.Equal(320, width, 3);
        Assert.Equal(240, height, 3);
    }

    [AvaloniaFact]
    public void PinDockable_UsesOwnerVisibleBounds_WhenDockableInvalid()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        tool.SetVisibleBounds(0, 0, double.NaN, double.NaN);
        toolDock.SetVisibleBounds(0, 0, 600, 400);

        factory.PinDockable(tool);

        tool.GetPinnedBounds(out _, out _, out var width, out var height);
        Assert.Equal(600, width, 3);
        Assert.Equal(400, height, 3);
    }
}
