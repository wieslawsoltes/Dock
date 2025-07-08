using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PreviewPinnedDockTests
{
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
        Assert.Equal(toolDock, tool.Owner);

        factory.HidePreviewingDockables(root);

        Assert.Empty(root.PinnedDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }
}
