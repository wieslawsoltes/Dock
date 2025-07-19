using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockManagerCrossDockTests
{
    [AvaloniaFact]
    public void ValidateTool_ReturnsTrue_When_Moving_Between_Docks()
    {
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(tool);
        targetDock.VisibleDockables!.Add(new Tool());

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsFalse_When_SizeConflict_And_PreventSizeConflicts()
    {
        var manager = new DockManager();
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock, MinWidth = 100, MaxWidth = 100 };
        sourceDock.VisibleDockables!.Add(tool);
        var existing = new Tool { Owner = targetDock, MinWidth = 200, MaxWidth = 200 };
        targetDock.VisibleDockables!.Add(existing);

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateTool_ReturnsTrue_When_SizeConflict_And_PreventSizeConflicts_Disabled()
    {
        var manager = new DockManager { PreventSizeConflicts = false };
        var sourceDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var tool = new Tool { Owner = sourceDock, MinWidth = 100, MaxWidth = 100 };
        sourceDock.VisibleDockables!.Add(tool);
        var existing = new Tool { Owner = targetDock, MinWidth = 200, MaxWidth = 200 };
        targetDock.VisibleDockables!.Add(existing);

        var result = manager.ValidateTool(tool, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ValidateDocument_ReturnsTrue_When_Moving_Between_DocumentDocks()
    {
        var manager = new DockManager();
        var sourceDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var doc = new Document { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(doc);

        var result = manager.ValidateDocument(doc, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ValidateDocument_ReturnsFalse_When_Target_Is_ToolDock()
    {
        var manager = new DockManager();
        var sourceDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new ToolDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var doc = new Document { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(doc);

        var result = manager.ValidateDocument(doc, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateDocument_ReturnsTrue_When_TargetDock_In_Different_Root()
    {
        var manager = new DockManager();
        var root1 = new RootDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var root2 = new RootDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var sourceDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var targetDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        root1.VisibleDockables!.Add(sourceDock);
        root2.VisibleDockables!.Add(targetDock);
        var doc = new Document { Owner = sourceDock };
        sourceDock.VisibleDockables!.Add(doc);

        var result = manager.ValidateDocument(doc, targetDock, DragAction.Move, DockOperation.Fill, false);

        Assert.True(result);
    }

    [AvaloniaFact(Skip = "TODO: pinned dock active state handling not implemented")]
    public void PinnedDock_Keeps_ActiveDockable_When_Pinned()
    {
    }
}
