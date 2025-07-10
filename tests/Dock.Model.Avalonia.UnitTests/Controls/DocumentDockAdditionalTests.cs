using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockAdditionalTests
{
    private class TrackingFactory : Factory
    {
        public IDockable? Added;
        public IDockable? Active;
        public (IDock Dock, IDockable? Dockable)? Focused;

        public override void AddDockable(IDock dock, IDockable dockable)
        {
            base.AddDockable(dock, dockable);
            Added = dockable;
        }

        public override void SetActiveDockable(IDockable dockable)
        {
            base.SetActiveDockable(dockable);
            Active = dockable;
        }

        public override void SetFocusedDockable(IDock dock, IDockable? dockable)
        {
            base.SetFocusedDockable(dock, dockable);
            Focused = (dock, dockable);
        }
    }

    [AvaloniaFact]
    public void AddDocument_AddsAndActivates()
    {
        var factory = new TrackingFactory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var document = new Document();

        dock.AddDocument(document);

        Assert.Contains(document, dock.VisibleDockables!);
        Assert.Equal(document, dock.ActiveDockable);
        Assert.Equal(document, factory.Added);
        Assert.Equal(document, factory.Active);
        Assert.Equal((dock, document), factory.Focused);
    }

    [AvaloniaFact]
    public void AddTool_AddsAndActivates()
    {
        var factory = new TrackingFactory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var tool = new Tool();

        dock.AddTool(tool);

        Assert.Contains(tool, dock.VisibleDockables!);
        Assert.Equal(tool, dock.ActiveDockable);
        Assert.Equal(tool, factory.Added);
        Assert.Equal(tool, factory.Active);
        Assert.Equal((dock, tool), factory.Focused);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_ReturnsNull_WhenTemplateMissing()
    {
        var factory = new TrackingFactory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true
        };

        var result = dock.CreateDocumentFromTemplate();

        Assert.Null(result);
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateDocumentFromTemplate_ReturnsNull_WhenNotAllowed()
    {
        var factory = new TrackingFactory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) },
            CanCreateDocument = false
        };

        var result = dock.CreateDocumentFromTemplate();

        Assert.Null(result);
        Assert.Empty(dock.VisibleDockables!);
    }
}
