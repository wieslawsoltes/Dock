using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Xunit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;

namespace Dock.Avalonia.HeadlessTests;

public class DockHelpersTests
{
    [AvaloniaFact]
    public void ToDockPoint_Returns_Correct_Point()
    {
        var point = new Point(10, 20);
        var dockPoint = DockHelpers.ToDockPoint(point);

        Assert.Equal(10, dockPoint.X);
        Assert.Equal(20, dockPoint.Y);
        Assert.Equal("10, 20", dockPoint.ToString());
    }

    [AvaloniaFact]
    public void FindProportionalDock_Finds_Child_Dock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { Factory = factory, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var proportional = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(docDock, proportional);

        var result = DockHelpers.FindProportionalDock(docDock);

        Assert.Same(proportional, result);
    }
}
