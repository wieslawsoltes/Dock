using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryDockableTests
{
    [AvaloniaFact]
    public void AddDockable_Adds_To_Dock_And_Sets_Owner()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;

        var dock = new DocumentDock();
        factory.AddDockable(root, dock);

        Assert.Single(root.VisibleDockables!);
        Assert.Equal(root, dock.Owner);
    }

    [AvaloniaFact]
    public void RemoveDockable_Removes_From_Dock()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        var doc = new Document();
        factory.AddDockable(dock, doc);

        factory.RemoveDockable(doc, false);

        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void SwapDockable_Swaps_Dockables_In_Same_Dock()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        var a = new Document { Title = "A" };
        var b = new Document { Title = "B" };
        factory.AddDockable(dock, a);
        factory.AddDockable(dock, b);

        factory.SwapDockable(dock, a, b);

        Assert.Equal(b, dock.VisibleDockables![0]);
        Assert.Equal(a, dock.VisibleDockables[1]);
    }

    [AvaloniaFact]
    public void MoveDockable_Moves_Dockable_In_Same_Dock()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        var a = new Document { Title = "A" };
        var b = new Document { Title = "B" };
        var c = new Document { Title = "C" };
        factory.AddDockable(dock, a);
        factory.AddDockable(dock, b);
        factory.AddDockable(dock, c);

        factory.MoveDockable(dock, c, a);

        Assert.Equal(new[] { c, a, b }, dock.VisibleDockables);
    }

    [AvaloniaFact]
    public void MoveDockable_Moves_To_Another_Dock()
    {
        var factory = new Factory();
        var source = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var target = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        source.Factory = factory;
        target.Factory = factory;
        var doc = new Document { Title = "Doc" };
        factory.AddDockable(source, doc);

        factory.MoveDockable(source, target, doc, null);

        Assert.Empty(source.VisibleDockables!);
        Assert.Single(target.VisibleDockables!);
        Assert.Equal(doc, target.VisibleDockables[0]);
        Assert.Equal(target, doc.Owner);
    }

    [AvaloniaFact]
    public void SwapDockable_Swaps_Between_Docks()
    {
        var factory = new Factory();
        var dockA = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var dockB = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dockA.Factory = factory;
        dockB.Factory = factory;
        var a = new Document { Title = "A" };
        var b = new Document { Title = "B" };
        factory.AddDockable(dockA, a);
        factory.AddDockable(dockB, b);

        factory.SwapDockable(dockA, dockB, a, b);

        Assert.Equal(b, dockA.VisibleDockables![0]);
        Assert.Equal(a, dockB.VisibleDockables![0]);
        Assert.Equal(dockA, b.Owner);
        Assert.Equal(dockB, a.Owner);
    }
}
