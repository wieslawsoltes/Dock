using System;
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
        Func<IDockable> documentFactory = () => new Document();
        docDock.DocumentFactory = documentFactory;
        factory.AddDockable(root, docDock);
        var doc = new Document();
        factory.AddDockable(docDock, doc);

        factory.NewHorizontalDocumentDock(doc);

        var ownerDock = Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, ownerDock);
        Assert.Contains(doc, ownerDock.VisibleDockables!);
        Assert.Same(documentFactory, ownerDock.DocumentFactory);
        Assert.NotNull(factory.FindDockable(root, d => ReferenceEquals(d, ownerDock)));
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

        var ownerDock = Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, ownerDock);
        Assert.Contains(doc, ownerDock.VisibleDockables!);
        Assert.NotNull(factory.FindDockable(root, d => ReferenceEquals(d, ownerDock)));
    }
}
