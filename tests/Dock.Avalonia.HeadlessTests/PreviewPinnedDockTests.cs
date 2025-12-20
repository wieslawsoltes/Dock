using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PreviewPinnedDockTests
{
    private static (Factory factory, RootDock root, ToolDock leftDock, ToolDock rightDock, Tool leftTool, Tool rightTool) CreatePinnedToolPair()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var leftDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        var rightDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, leftDock);
        factory.AddDockable(root, rightDock);

        var leftTool = new Tool { CanDrag = true, CanDrop = true };
        var rightTool = new Tool { CanDrag = true, CanDrop = true };
        factory.AddDockable(leftDock, leftTool);
        factory.AddDockable(rightDock, rightTool);

        factory.PinDockable(leftTool);
        factory.PinDockable(rightTool);

        return (factory, root, leftDock, rightDock, leftTool, rightTool);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_Shows_In_PinnedDock_And_Hides()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(tool, root.PinnedDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.OriginalOwner);
        Assert.Equal(root.PinnedDock, tool.Owner);

        factory.HidePreviewingDockables(root);

        Assert.Null(root.PinnedDock);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void HidePreviewingDockables_Keeps_KeepPinnedDockableVisible()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool { KeepPinnedDockableVisible = true };
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);
        factory.HidePreviewingDockables(root);

        Assert.NotNull(root.PinnedDock);
        Assert.Contains(tool, root.PinnedDock!.VisibleDockables!);
        Assert.Equal(root.PinnedDock, tool.Owner);
        Assert.Equal(toolDock, tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_Replaces_KeepVisible_When_Previewing_Another_Pinned()
    {
        var (factory, root, leftDock, rightDock, leftTool, rightTool) = CreatePinnedToolPair();
        leftTool.KeepPinnedDockableVisible = true;

        factory.PreviewPinnedDockable(leftTool);
        factory.PreviewPinnedDockable(rightTool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(rightTool, root.PinnedDock.VisibleDockables!);
        Assert.DoesNotContain(leftTool, root.PinnedDock.VisibleDockables!);
        Assert.Equal(leftDock, leftTool.Owner);
        Assert.Null(leftTool.OriginalOwner);
        Assert.Equal(root.PinnedDock, rightTool.Owner);
        Assert.Equal(rightDock, rightTool.OriginalOwner);
    }

    [AvaloniaFact]
    public void UnpinDockable_Hides_KeepPinnedDockableVisible_Preview()
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
        var tool = new Tool { KeepPinnedDockableVisible = true };
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Contains(tool, root.PinnedDock!.VisibleDockables!);

        factory.UnpinDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.DoesNotContain(tool, root.LeftPinnedDockables!);
    }
}
