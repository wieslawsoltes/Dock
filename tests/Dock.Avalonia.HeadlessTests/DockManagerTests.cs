using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia;
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
        var manager = new DockManager();
        Assert.True(manager.PreventSizeConflicts);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsFalse_When_SourceCannotDrag()
    {
        var manager = new DockManager();
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
        var manager = new DockManager();
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
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        var targetDock = new ToolDock { VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>() };

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);
        Assert.True(result);
    }

    [AvaloniaFact]
    public void DockDockable_Fill_Moves_To_TargetDock()
    {
        var manager = new DockManager();
        var factory = new Factory();
        var sourceDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>(), Factory = factory };
        var targetDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>(), Factory = factory };
        var tool = new Tool();
        factory.AddDockable(sourceDock, tool);

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.DoesNotContain(tool, sourceDock.VisibleDockables!);
        Assert.Contains(tool, targetDock.VisibleDockables!);
        Assert.Same(targetDock, tool.Owner);
    }
}
