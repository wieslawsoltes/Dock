using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Avalonia.Controls;
using Xunit;
using Avalonia.Collections;

namespace Dock.Avalonia.HeadlessTests;

public class DockManagerTests
{
    [AvaloniaFact]
    public void PreventSizeConflicts_Default_True()
    {
        var manager = new DockManager(new DockService());
        Assert.True(manager.PreventSizeConflicts);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsFalse_When_SourceCannotDrag()
    {
        var manager = new DockManager(new DockService());
        var sourceDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };
        var tool = new Tool { CanDrag = false, Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>(), CanDrop = true };

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsFalse_When_TargetCannotDrop()
    {
        var manager = new DockManager(new DockService());
        var sourceDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>(), CanDrop = false };

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsTrue_When_Valid()
    {
        var manager = new DockManager(new DockService());
        var sourceDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result);
    }
}
