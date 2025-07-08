using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryPinAlignmentTests
{
    [AvaloniaFact]
    public void PinDockable_Right_Adds_To_RightPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);

        Assert.Single(root.RightPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.True(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void UnpinDockable_Right_Removes_From_RightPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.UnpinDockable(tool);

        Assert.Empty(root.RightPinnedDockables!);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.False(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void PinDockable_Top_Adds_To_TopPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            TopPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Top, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);

        Assert.Single(root.TopPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.True(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void UnpinDockable_Top_Removes_From_TopPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            TopPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Top, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.UnpinDockable(tool);

        Assert.Empty(root.TopPinnedDockables!);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.False(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void PinDockable_Bottom_Adds_To_BottomPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            BottomPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Bottom, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);

        Assert.Single(root.BottomPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.True(factory.IsDockablePinned(tool, root));
    }

    [AvaloniaFact]
    public void UnpinDockable_Bottom_Removes_From_BottomPinned_List()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            BottomPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Bottom, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.UnpinDockable(tool);

        Assert.Empty(root.BottomPinnedDockables!);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.False(factory.IsDockablePinned(tool, root));
    }
}
