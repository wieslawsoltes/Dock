using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentDockTemplateTests
{
    [AvaloniaFact]
    public void CreateDocumentFromTemplate_Returns_Null_When_No_Template()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = null
        };

        var document = dock.CreateDocumentFromTemplate();

        Assert.Null(document);
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_Returns_Null_When_CanCreateDocument_False()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = false,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };

        var document = dock.CreateDocumentFromTemplate();

        Assert.Null(document);
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_Adds_Multiple_Documents()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };

        var doc1 = dock.CreateDocumentFromTemplate();
        var doc2 = dock.CreateDocumentFromTemplate();

        Assert.Equal(2, dock.VisibleDockables!.Count);
        Assert.Contains(doc1, dock.VisibleDockables);
        Assert.Contains(doc2, dock.VisibleDockables);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_Sets_Active_And_Focused_Dockables()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };
        factory.AddDockable(root, dock);

        var doc = dock.CreateDocumentFromTemplate();

        Assert.Equal(doc, dock.ActiveDockable);
        Assert.Equal(doc, root.FocusedDockable);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_Increments_Title()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };

        var doc1 = (Document)dock.CreateDocumentFromTemplate()!;
        var doc2 = (Document)dock.CreateDocumentFromTemplate()!;

        Assert.Equal("Document0", doc1.Title);
        Assert.Equal("Document1", doc2.Title);
    }
}
