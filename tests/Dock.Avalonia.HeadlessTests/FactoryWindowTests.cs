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
        var tool = new Tool { Id = "ToolA" };
        var sourceDock = new ToolDock
        {
            Id = "SourceToolDock",
            Alignment = Alignment.Right,
            IsExpanded = true,
            AutoHide = false,
            GripMode = GripMode.Hidden,
            ToolTemplate = toolTemplate,
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
    }
}
