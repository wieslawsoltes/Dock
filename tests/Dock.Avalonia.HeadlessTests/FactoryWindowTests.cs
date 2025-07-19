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
        var docDock = Assert.IsType<DocumentDock>(root.VisibleDockables![0]);
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
        Assert.Same(toolDock, root.VisibleDockables![0]);
    }
}
