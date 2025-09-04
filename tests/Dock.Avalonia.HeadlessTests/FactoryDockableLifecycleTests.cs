using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryDockableLifecycleTests
{
    [AvaloniaFact]
    public void HideDockable_Moves_Dockable_To_HiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            HiddenDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var doc = new Document();
        factory.AddDockable(root, doc);

        factory.HideDockable(doc);

        Assert.DoesNotContain(doc, root.VisibleDockables!);
        Assert.Single(root.HiddenDockables!);
        Assert.Contains(doc, root.HiddenDockables);
    }

    [AvaloniaFact]
    public void RestoreDockable_Moves_Dockable_Back_To_Original_Owner()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            HiddenDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var doc = new Document();
        factory.AddDockable(root, doc);

        factory.HideDockable(doc);
        factory.RestoreDockable(doc);

        Assert.Contains(doc, root.VisibleDockables!);
        Assert.Empty(root.HiddenDockables!);
        Assert.Null(doc.OriginalOwner);
    }

    [AvaloniaFact]
    public void CloseDockable_Removes_From_Dock()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        var doc = new Document();
        factory.AddDockable(dock, doc);

        factory.CloseDockable(doc);

        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CloseDockable_NonActive_Does_Not_Change_ActiveDockable()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;

        var doc1 = new Document();
        var doc2 = new Document();
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, doc2);

        dock.ActiveDockable = doc1;

        factory.CloseDockable(doc2);

        Assert.Same(doc1, dock.ActiveDockable);
        Assert.Single(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CloseDockable_Active_Selects_Neighbour()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;

        var doc1 = new Document();
        var doc2 = new Document();
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, doc2);

        dock.ActiveDockable = doc1;

        factory.CloseDockable(doc1);

        Assert.Same(doc2, dock.ActiveDockable);
        Assert.Single(dock.VisibleDockables!);
    }
}
