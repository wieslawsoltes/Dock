using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockManagerDocumentTests
{
    [AvaloniaFact]
    public void ValidateDocument_ReturnsFalse_When_SourceCannotDrag()
    {
        var manager = new DockManager(new DockService());
        var source = new Document { CanDrag = false };
        var target = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>(), CanDrop = true };

        var result = manager.ValidateDocument(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateDocument_ReturnsFalse_When_TargetCannotDrop()
    {
        var manager = new DockManager(new DockService());
        var source = new Document();
        var target = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>(), CanDrop = false };

        var result = manager.ValidateDocument(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateDocument_ReturnsFalse_When_SourceCannotDockAsDocument_And_TargetIsDocumentDock()
    {
        var manager = new DockManager(new DockService());
        var sourceDock = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>() };
        var source = new Document { Owner = sourceDock, CanDockAsDocument = false };
        sourceDock.VisibleDockables!.Add(source);
        var target = new DocumentDock { VisibleDockables = new AvaloniaList<IDockable>(), CanDrop = true };

        var result = manager.ValidateDocument(source, target, DragAction.Move, DockOperation.Fill, false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void IsDockTargetVisible_ReturnsFalse_For_Same_Dockable()
    {
        var manager = new DockManager(new DockService());
        var doc = new Document();

        var result = manager.IsDockTargetVisible(doc, doc, DockOperation.Fill);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void IsDockTargetVisible_ReturnsFalse_When_Target_Is_Owner()
    {
        var manager = new DockManager(new DockService());
        var dock = new DocumentDock();
        var doc = new Document { Owner = dock };

        var result = manager.IsDockTargetVisible(doc, dock, DockOperation.Fill);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void IsDockTargetVisible_ReturnsTrue_For_Different_Dockables()
    {
        var manager = new DockManager(new DockService());
        var doc1 = new Document();
        var doc2 = new Document();

        var result = manager.IsDockTargetVisible(doc1, doc2, DockOperation.Fill);

        Assert.True(result);
    }
}
