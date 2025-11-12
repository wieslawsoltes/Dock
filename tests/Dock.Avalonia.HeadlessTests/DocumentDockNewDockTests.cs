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

    [AvaloniaFact]
    public void NewHorizontalDocumentDock_With_Single_Document_Preserves_Old_Dock()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var doc = new Document { Title = "TestDoc" };
        factory.AddDockable(docDock, doc);

        // Act
        factory.NewHorizontalDocumentDock(doc);

        // Assert: Document should be in a new dock
        Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, doc.Owner);
        var newDock = (DocumentDock)doc.Owner!;
        Assert.Contains(doc, newDock.VisibleDockables!);

        // Assert: Root should contain a ProportionalDock with the old (now empty) dock and new dock
        Assert.Single(root.VisibleDockables!);
        Assert.IsType<ProportionalDock>(root.VisibleDockables![0]);
        var proportional = (ProportionalDock)root.VisibleDockables![0];
        
        // The proportional dock preserves the empty dock so users can add new documents to it
        // It should have: docDock (empty), splitter, newDock (with doc)
        Assert.Equal(3, proportional.VisibleDockables!.Count);
        Assert.Contains(docDock, proportional.VisibleDockables!);
        Assert.Contains(newDock, proportional.VisibleDockables!);
        
        // The old dock should be empty
        Assert.Empty(docDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void NewVerticalDocumentDock_With_Single_Document_Preserves_Old_Dock()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var doc = new Document { Title = "TestDoc" };
        factory.AddDockable(docDock, doc);

        // Act
        factory.NewVerticalDocumentDock(doc);

        // Assert: Document should be in a new dock
        Assert.IsType<DocumentDock>(doc.Owner);
        Assert.NotEqual(docDock, doc.Owner);
        var newDock = (DocumentDock)doc.Owner!;
        Assert.Contains(doc, newDock.VisibleDockables!);

        // Assert: Root should contain a ProportionalDock with the old (now empty) dock and new dock
        Assert.Single(root.VisibleDockables!);
        Assert.IsType<ProportionalDock>(root.VisibleDockables![0]);
        var proportional = (ProportionalDock)root.VisibleDockables![0];
        
        // The proportional dock preserves the empty dock so users can add new documents to it
        // It should have: docDock (empty), splitter, newDock (with doc)
        Assert.Equal(3, proportional.VisibleDockables!.Count);
        Assert.Contains(docDock, proportional.VisibleDockables!);
        Assert.Contains(newDock, proportional.VisibleDockables!);
        
        // The old dock should be empty
        Assert.Empty(docDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void NewHorizontalDocumentDock_With_Multiple_Documents_Creates_Split()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var doc1 = new Document { Title = "Doc1" };
        var doc2 = new Document { Title = "Doc2" };
        factory.AddDockable(docDock, doc1);
        factory.AddDockable(docDock, doc2);

        // Act
        factory.NewHorizontalDocumentDock(doc1);

        // Assert: doc1 should be in a new dock
        Assert.IsType<DocumentDock>(doc1.Owner);
        Assert.NotEqual(docDock, doc1.Owner);
        var newDock = (DocumentDock)doc1.Owner!;
        Assert.Contains(doc1, newDock.VisibleDockables!);
        
        // Assert: doc2 should remain in the original dock
        Assert.Equal(docDock, doc2.Owner);
        Assert.Contains(doc2, docDock.VisibleDockables!);

        // Assert: Root should contain a ProportionalDock with both docks
        Assert.Single(root.VisibleDockables!);
        Assert.IsType<ProportionalDock>(root.VisibleDockables![0]);
        var proportional = (ProportionalDock)root.VisibleDockables![0];
        
        // The proportional dock should have the old dock (with doc2) and new dock (with doc1) 
        // with a splitter between them
        Assert.Equal(3, proportional.VisibleDockables!.Count); // dock, splitter, newDock
        Assert.Contains(docDock, proportional.VisibleDockables!);
        Assert.Contains(newDock, proportional.VisibleDockables!);
    }
}
