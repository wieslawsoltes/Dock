using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryPinTests
{
    [AvaloniaFact]
    public void PinDockable_Adds_To_Root_Pinned_List()
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

        factory.PinDockable(tool);

        Assert.Single(root.LeftPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.True(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void UnpinDockable_Removes_From_Root_Pinned_List()
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

        factory.PinDockable(tool);
        factory.UnpinDockable(tool);

        Assert.Empty(root.LeftPinnedDockables!);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.False(factory.IsDockablePinned(tool, root));
    }
}
