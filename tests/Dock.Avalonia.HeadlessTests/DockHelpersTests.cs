using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Xunit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using System.Linq;

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

    [AvaloniaFact]
    public void IsNestedWithin_Returns_False_For_Same_DockControl()
    {
        var dockControl = new DockControl();

        var result = DockHelpers.IsNestedWithin(dockControl, dockControl);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void GetRelevantDockControls_Returns_Active_When_No_Other_DockControls()
    {
        var dockControl = new DockControl();
        var dockControls = new System.Collections.Generic.List<IDockControl> { dockControl };

        var relevant = DockHelpers.GetRelevantDockControls(dockControls, dockControl).ToList();

        Assert.Single(relevant);
        Assert.Contains(dockControl, relevant);
    }

    [AvaloniaFact]
    public void GetRelevantDockControls_Returns_All_When_No_Nesting()
    {
        var dock1 = new DockControl();
        var dock2 = new DockControl();
        var dockControls = new System.Collections.Generic.List<IDockControl> { dock1, dock2 };

        var relevantForDock1 = DockHelpers.GetRelevantDockControls(dockControls, dock1).ToList();
        var relevantForDock2 = DockHelpers.GetRelevantDockControls(dockControls, dock2).ToList();

        // Both docks should see each other when not nested
        Assert.Equal(2, relevantForDock1.Count);
        Assert.Contains(dock1, relevantForDock1);
        Assert.Contains(dock2, relevantForDock1);

        Assert.Equal(2, relevantForDock2.Count);
        Assert.Contains(dock1, relevantForDock2);
        Assert.Contains(dock2, relevantForDock2);
    }
}
