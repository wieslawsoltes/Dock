using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolDockItemsSourceTests
{
    private sealed class ToolItem
    {
        public string Title { get; init; } = string.Empty;

        public bool CanClose { get; init; } = true;
    }

    [AvaloniaFact]
    public void ItemsSource_WithTemplate_GeneratesToolsWithTemplateContent()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate
            {
                Content = (Func<IServiceProvider, object>)(_ => new TextBlock { Text = "Tool content" })
            }
        };

        dock.ItemsSource = new[] { new ToolItem { Title = "Tool1" } };

        Assert.NotNull(dock.VisibleDockables);
        Assert.Single(dock.VisibleDockables!);

        var tool = Assert.IsType<Tool>(dock.VisibleDockables![0]);
        Assert.Equal("Tool1", tool.Title);
        Assert.NotNull(tool.Content);

        var contentFactory = Assert.IsType<Func<IServiceProvider, object>>(tool.Content);
        var content = contentFactory(null!);
        var textBlock = Assert.IsType<TextBlock>(content);
        Assert.Equal("Tool content", textBlock.Text);
    }

    [AvaloniaFact]
    public void CloseGeneratedTool_RemovesSourceItem()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var source = new ObservableCollection<ToolItem>
        {
            new() { Title = "Tool1" },
            new() { Title = "Tool2" }
        };

        dock.ItemsSource = source;

        Assert.NotNull(dock.VisibleDockables);
        Assert.Equal(2, dock.VisibleDockables!.Count);

        var tool = Assert.IsType<Tool>(dock.VisibleDockables![0]);
        factory.CloseDockable(tool);

        Assert.Single(source);
        Assert.Single(dock.VisibleDockables!);
        var remaining = Assert.IsType<Tool>(dock.VisibleDockables![0]);
        Assert.Equal("Tool2", remaining.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WithDockControlLayout_WorksWithAvaloniaModel()
    {
        var factory = new Factory
        {
            HideToolsOnClose = true
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            ToolTemplate = new ToolTemplate
            {
                Content = (Func<IServiceProvider, object>)(_ => new TextBlock { Text = "Tool content" })
            }
        };

        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(toolDock),
            DefaultDockable = toolDock,
            ActiveDockable = toolDock
        };

        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = true,
            InitializeLayout = true
        };

        factory.InitLayout(root);

        var source = new ObservableCollection<ToolItem>
        {
            new() { Title = "Tool1" },
            new() { Title = "Tool2" }
        };

        toolDock.ItemsSource = source;
        Assert.NotNull(toolDock.VisibleDockables);
        Assert.Equal(2, toolDock.VisibleDockables!.Count);

        var generated = Assert.IsType<Tool>(toolDock.VisibleDockables[0]);
        factory.CloseDockable(generated);

        Assert.Single(source);
        Assert.Single(toolDock.VisibleDockables);
        Assert.True(root.HiddenDockables == null || root.HiddenDockables.Count == 0);

        // Keep control alive for the full test to validate DockControl integration path.
        Assert.NotNull(dockControl.Layout);
    }

    [AvaloniaFact]
    public void CloseMovedGeneratedTool_WithDockControlLayout_RemovesSourceItem()
    {
        var factory = new Factory();
        var sourceDock = new ToolDock
        {
            Id = "SourceTools",
            ToolTemplate = new ToolTemplate()
        };

        var targetDock = new ToolDock
        {
            Id = "TargetTools",
            ToolTemplate = new ToolTemplate()
        };

        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(sourceDock, targetDock),
            DefaultDockable = sourceDock,
            ActiveDockable = sourceDock
        };

        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = true,
            InitializeLayout = true
        };

        factory.InitLayout(root);

        var source = new ObservableCollection<ToolItem>
        {
            new() { Title = "Tool1" }
        };

        sourceDock.ItemsSource = source;
        var generatedTool = Assert.IsType<Tool>(sourceDock.VisibleDockables![0]);

        factory.MoveDockable(sourceDock, targetDock, generatedTool, null);
        Assert.NotNull(root.VisibleDockables);
        Assert.DoesNotContain(sourceDock, root.VisibleDockables!);
        Assert.Single(targetDock.VisibleDockables!);

        factory.CloseDockable(generatedTool);

        Assert.Empty(source);
        Assert.Empty(targetDock.VisibleDockables!);
        Assert.NotNull(dockControl.Layout);
    }

    [AvaloniaFact]
    public void RemoveMovedSourceItem_WithDockControlLayout_RemovesToolFromTargetDock()
    {
        var factory = new Factory();
        var sourceDock = new ToolDock
        {
            Id = "SourceTools",
            ToolTemplate = new ToolTemplate()
        };

        var targetDock = new ToolDock
        {
            Id = "TargetTools",
            ToolTemplate = new ToolTemplate()
        };

        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(sourceDock, targetDock),
            DefaultDockable = sourceDock,
            ActiveDockable = sourceDock
        };

        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = true,
            InitializeLayout = true
        };

        factory.InitLayout(root);

        var item = new ToolItem { Title = "Tool1" };
        var source = new ObservableCollection<ToolItem> { item };

        sourceDock.ItemsSource = source;
        var generatedTool = Assert.IsType<Tool>(sourceDock.VisibleDockables![0]);

        factory.MoveDockable(sourceDock, targetDock, generatedTool, null);
        Assert.NotNull(root.VisibleDockables);
        Assert.DoesNotContain(sourceDock, root.VisibleDockables!);
        Assert.Single(targetDock.VisibleDockables!);

        source.Remove(item);

        Assert.Empty(targetDock.VisibleDockables!);
        Assert.NotNull(dockControl.Layout);
    }
}
