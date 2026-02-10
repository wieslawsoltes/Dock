using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolDockTemplateTests
{
    [AvaloniaFact]
    public void CreateToolFromTemplate_ReturnsNull_WhenNoTemplate()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            ToolTemplate = null
        };

        var tool = dock.CreateToolFromTemplate();

        Assert.Null(tool);
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateToolFromTemplate_AddsTool_WhenTemplateProvided()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            ToolTemplate = new ToolTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };

        var tool = dock.CreateToolFromTemplate();

        Assert.NotNull(tool);
        Assert.IsType<Tool>(tool);
        Assert.Single(dock.VisibleDockables!);
    }
}
