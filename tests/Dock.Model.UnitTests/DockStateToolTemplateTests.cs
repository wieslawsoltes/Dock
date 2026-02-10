using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockStateToolTemplateTests
{
    [Fact]
    public void SaveRestore_IncludesToolTemplate()
    {
        var factory = new Factory();
        var tool = new Tool { Id = "ToolA", Content = "payload" };
        var toolTemplate = new ToolTemplate { Content = "template" };

        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            ToolTemplate = toolTemplate,
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        var state = new DockState();
        state.Save(root);

        toolDock.ToolTemplate = null;
        state.Restore(root);

        Assert.Same(toolTemplate, toolDock.ToolTemplate);
    }

    [Fact]
    public void Restore_ToolContent_FallsBackToToolTemplate_WhenNoSavedContent()
    {
        var factory = new Factory();
        var tool = new Tool { Id = "ToolA", Content = null };
        var toolTemplate = new ToolTemplate { Content = "template-content" };

        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            ToolTemplate = toolTemplate,
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        var state = new DockState();
        state.Restore(root);

        Assert.Equal("template-content", tool.Content);
    }
}
