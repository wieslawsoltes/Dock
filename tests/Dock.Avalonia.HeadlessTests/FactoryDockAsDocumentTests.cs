using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryDockAsDocumentTests
{
    [AvaloniaFact]
    public void DockAsDocument_DoesNothing_When_DockableCannotDockAsDocument()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;

        var documentDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        var toolDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };

        factory.AddDockable(root, documentDock);
        factory.AddDockable(root, toolDock);

        var tool = new Tool { CanDockAsDocument = false };
        factory.AddDockable(toolDock, tool);

        factory.DockAsDocument(tool);

        Assert.Same(toolDock, tool.Owner);
        Assert.DoesNotContain(tool, documentDock.VisibleDockables!);
    }
}
