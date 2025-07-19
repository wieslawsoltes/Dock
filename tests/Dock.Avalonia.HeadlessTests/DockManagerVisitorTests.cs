using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockManagerVisitorTests
{
    [AvaloniaFact]
    public void ValidateDockable_Tool_ReturnsFalse_When_SourceCannotDrag()
    {
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { CanDrag = false, Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>(), CanDrop = true };

        var result = manager.ValidateDockable(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateDockable_Tool_ReturnsFalse_When_TargetCannotDrop()
    {
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>(), CanDrop = false };

        var result = manager.ValidateDockable(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateDockable_Tool_ReturnsTrue_When_Valid()
    {
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };

        var result = manager.ValidateDockable(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result);
    }
}

