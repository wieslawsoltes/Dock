using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using System.Linq;

namespace Dock.Avalonia.HeadlessTests;

public class FactorySplitterEdgeCaseTests
{
    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableNotInVisibleDockables()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc = new Document { Title = "Doc" };
        // Don't add the dockable to the dock

        // Should not throw when dockable is not in VisibleDockables
        factory.RemoveDockable(doc, false);
        
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableWithNullOwner()
    {
        var factory = new Factory();
        var doc = new Document { Title = "Doc" };
        doc.Owner = null;

        // Should not throw when dockable has null owner
        factory.RemoveDockable(doc, false);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockWithNullVisibleDockables()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = null };
        dock.Factory = factory;
        
        var doc = new Document { Title = "Doc" };
        doc.Owner = dock;

        // Should not throw when dock has null VisibleDockables
        factory.RemoveDockable(doc, false);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesAllSplittersLayout()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var splitter1 = factory.CreateProportionalDockSplitter();
        var splitter2 = factory.CreateProportionalDockSplitter();
        var splitter3 = factory.CreateProportionalDockSplitter();
        
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, splitter3);

        factory.RemoveDockable(splitter1, false);

        // Should clean up all splitters when only splitters remain
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesAlternatingSplittersAndDockables()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc1 = new Document { Title = "Doc1" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        var splitter3 = factory.CreateProportionalDockSplitter();
        
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc2);
        factory.AddDockable(dock, splitter3);

        factory.RemoveDockable(doc1, false);
        factory.RemoveDockable(doc2, false);

        // Should clean up all splitters when no dockables remain
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableAtBeginning()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter);
        factory.AddDockable(dock, doc2);

        factory.RemoveDockable(doc1, false);

        // Should remove the splitter at the beginning
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc2, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableAtEnd()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter);
        factory.AddDockable(dock, doc2);

        factory.RemoveDockable(doc2, false);

        // Should remove the splitter at the end
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc1, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableInMiddle()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var doc3 = new Document { Title = "Doc3" };
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc2);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc3);

        factory.RemoveDockable(doc2, false);

        // Should remove both splitters around the middle dockable
        Assert.Equal(3, dock.VisibleDockables!.Count);
        Assert.Equal(doc1, dock.VisibleDockables[0]);
        Assert.Equal(splitter1, dock.VisibleDockables[1]);
        Assert.Equal(doc3, dock.VisibleDockables[2]);
    }

    [AvaloniaFact]
    public void RemoveDockable_HandlesDockableWithPinnedState()
    {
        var factory = new Factory();
        var root = new RootDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        factory.AddDockable(dock, doc);

        // Pin the dockable
        factory.PinDockable(doc);

        factory.RemoveDockable(doc, false);

        // Should handle pinned dockable removal
        Assert.Empty(dock.VisibleDockables!);
        Assert.Empty(root.LeftPinnedDockables!);
    }
} 