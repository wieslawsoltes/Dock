using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentDockNewDockTests
{
    [AvaloniaFact]
    public void NewHorizontalDocumentDock_Moves_Document_To_New_Dock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var doc = new Document();
        factory.AddDockable(docDock, doc);

        factory.NewHorizontalDocumentDock(doc);

        Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, doc.Owner);
        Assert.Contains(doc, ((DocumentDock)doc.Owner!).VisibleDockables!);
    }

    [AvaloniaFact]
    public void NewVerticalDocumentDock_Moves_Document_To_New_Dock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var doc = new Document();
        factory.AddDockable(docDock, doc);

        factory.NewVerticalDocumentDock(doc);

        Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, doc.Owner);
        Assert.Contains(doc, ((DocumentDock)doc.Owner!).VisibleDockables!);
    }
}
