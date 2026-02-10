using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryWindowTests
{
    private sealed class NoOpDocumentSelector : IDocumentItemTemplateSelector
    {
        public object? SelectTemplate(IItemsSourceDock dock, object item, int index) => null;
    }

    private sealed class NoOpToolSelector : IToolItemTemplateSelector
    {
        public object? SelectTemplate(IToolItemsSourceDock dock, object item, int index) => null;
    }

    [AvaloniaFact]
    public void CreateWindowFrom_Document_Returns_Window_With_Document()
    {
        var factory = new Factory();
        var document = new Document();
        var window = factory.CreateWindowFrom(document);

        Assert.NotNull(window);
        Assert.IsType<DockWindow>(window);
        var root = Assert.IsAssignableFrom<IRootDock>(window!.Layout);
        Assert.Single(root.VisibleDockables!);
        var docDock = Assert.IsType<DocumentDock>(root.VisibleDockables?[0]);
        Assert.Contains(document, docDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateWindowFrom_ToolDock_Returns_Window_With_Dock()
    {
        var factory = new Factory();
        var toolDock = new ToolDock();
        var window = factory.CreateWindowFrom(toolDock);

        Assert.NotNull(window);
        var root = Assert.IsAssignableFrom<IRootDock>(window!.Layout);
        Assert.Single(root.VisibleDockables!);
        Assert.Same(toolDock, root.VisibleDockables?[0]);
    }

    [AvaloniaFact]
    public void CreateWindowFrom_Tool_CopiesOwnerToolDockTemplateAndSettings()
    {
        var factory = new Factory();
        var toolTemplate = new ToolTemplate { Content = "tool-template" };
        var toolSelector = new NoOpToolSelector();
        var tool = new Tool { Id = "ToolA" };
        var sourceDock = new ToolDock
        {
            Id = "SourceToolDock",
            Alignment = Alignment.Right,
            IsExpanded = true,
            AutoHide = false,
            GripMode = GripMode.Hidden,
            ToolTemplate = toolTemplate,
            ToolItemContainerTheme = "ToolTheme",
            ToolItemTemplateSelector = toolSelector,
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(sourceDock);
        root.ActiveDockable = sourceDock;
        factory.InitLayout(root);

        var window = factory.CreateWindowFrom(tool);
        Assert.NotNull(window);

        var windowRoot = Assert.IsAssignableFrom<IRootDock>(window!.Layout);
        var createdToolDock = Assert.IsType<ToolDock>(windowRoot.VisibleDockables?[0]);
        Assert.Equal(sourceDock.Id, createdToolDock.Id);
        Assert.Equal(sourceDock.Alignment, createdToolDock.Alignment);
        Assert.Equal(sourceDock.IsExpanded, createdToolDock.IsExpanded);
        Assert.Equal(sourceDock.AutoHide, createdToolDock.AutoHide);
        Assert.Equal(sourceDock.GripMode, createdToolDock.GripMode);
        Assert.Same(toolTemplate, createdToolDock.ToolTemplate);
        Assert.Equal("ToolTheme", createdToolDock.ToolItemContainerTheme);
        Assert.Same(toolSelector, createdToolDock.ToolItemTemplateSelector);
    }

    [AvaloniaFact]
    public void CreateWindowFrom_Document_CopiesOwnerDocumentDockTemplateAndSettings()
    {
        var factory = new Factory();
        var documentTemplate = new DocumentTemplate { Content = "document-template" };
        var documentSelector = new NoOpDocumentSelector();
        var document = new Document { Id = "DocumentA" };
        var sourceDock = new DocumentDock
        {
            Id = "SourceDocumentDock",
            CanCreateDocument = true,
            EnableWindowDrag = true,
            DocumentTemplate = documentTemplate,
            DocumentItemContainerTheme = "DocumentTheme",
            DocumentItemTemplateSelector = documentSelector,
            VisibleDockables = factory.CreateList<IDockable>(document),
            ActiveDockable = document
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(sourceDock);
        root.ActiveDockable = sourceDock;
        factory.InitLayout(root);

        var window = factory.CreateWindowFrom(document);
        Assert.NotNull(window);

        var windowRoot = Assert.IsAssignableFrom<IRootDock>(window!.Layout);
        var createdDocumentDock = Assert.IsType<DocumentDock>(windowRoot.VisibleDockables?[0]);
        Assert.Equal(sourceDock.Id, createdDocumentDock.Id);
        Assert.Equal(sourceDock.CanCreateDocument, createdDocumentDock.CanCreateDocument);
        Assert.Equal(sourceDock.EnableWindowDrag, createdDocumentDock.EnableWindowDrag);
        Assert.Same(documentTemplate, createdDocumentDock.DocumentTemplate);
        Assert.Equal("DocumentTheme", createdDocumentDock.DocumentItemContainerTheme);
        Assert.Same(documentSelector, createdDocumentDock.DocumentItemTemplateSelector);
    }
}
