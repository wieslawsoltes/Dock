using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using System.Linq;

namespace Dock.Avalonia.HeadlessTests;

public class FactorySplitterCleanupTests
{
    [AvaloniaFact]
    public void RemoveDockable_CleansUp_OrphanedSplitters_AtBeginning()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc1 = new Document { Title = "Doc1" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc2);

        factory.RemoveDockable(doc1, false);

        // Should remove the splitter at the beginning
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc2, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_CleansUp_OrphanedSplitters_AtEnd()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc2);
        factory.AddDockable(dock, splitter2);

        factory.RemoveDockable(doc2, false);

        // Should remove the splitter at the end
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc1, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_CleansUp_ConsecutiveSplitters()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter1 = factory.CreateProportionalDockSplitter();
        var splitter2 = factory.CreateProportionalDockSplitter(); // Consecutive splitter
        var doc2 = new Document { Title = "Doc2" };
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc2);

        factory.RemoveDockable(doc1, false);

        // Should remove consecutive splitters, keeping only one
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc2, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_CleansUp_SingleSplitterRemaining()
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
        factory.RemoveDockable(doc2, false);

        // Should remove the splitter when only it remains
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_Preserves_ValidSplitterLayout()
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

        // Should preserve the valid splitter layout
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc2, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RemoveDockable_Middle_Dockable_Preserves_Splitter()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;

        var doc0 = new Document { Title = "Doc0" };
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc1 = new Document { Title = "Doc1" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };

        factory.AddDockable(dock, doc0);
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc2);

        factory.RemoveDockable(doc1, false);

        Assert.Equal(3, dock.VisibleDockables!.Count);
        Assert.Equal(doc0, dock.VisibleDockables[0]);
        Assert.IsAssignableFrom<ISplitter>(dock.VisibleDockables[1]);
        Assert.Equal(doc2, dock.VisibleDockables[2]);
    }

    [AvaloniaFact]
    public void RemoveDockable_Handles_ComplexSplitterLayout()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        var splitter1 = factory.CreateProportionalDockSplitter();
        var doc1 = new Document { Title = "Doc1" };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        var splitter3 = factory.CreateProportionalDockSplitter();
        var splitter4 = factory.CreateProportionalDockSplitter(); // Consecutive
        var doc3 = new Document { Title = "Doc3" };
        var splitter5 = factory.CreateProportionalDockSplitter();
        
        factory.AddDockable(dock, splitter1);
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter2);
        factory.AddDockable(dock, doc2);
        factory.AddDockable(dock, splitter3);
        factory.AddDockable(dock, splitter4);
        factory.AddDockable(dock, doc3);
        factory.AddDockable(dock, splitter5);

        factory.RemoveDockable(doc1, false);
        factory.RemoveDockable(doc2, false);
        factory.RemoveDockable(doc3, false);

        // Should clean up all orphaned splitters
        Assert.Empty(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_WithCollapse_TriggersCollapseDock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            IsCollapsable = true
        };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        factory.AddDockable(dock, doc);

        factory.RemoveDockable(doc, true);

        // Should collapse the dock when collapse is true
        Assert.Empty(root.VisibleDockables!);
    }

    [AvaloniaFact]
    public void RemoveDockable_WithoutCollapse_DoesNotTriggerCollapseDock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            IsCollapsable = true
        };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        factory.AddDockable(dock, doc);

        factory.RemoveDockable(doc, false);

        // Should not collapse the dock when collapse is false
        Assert.Single(root.VisibleDockables!);
        Assert.Equal(dock, root.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void CleanupOrphanedSplitters_HandlesNullVisibleDockables()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = null };
        dock.Factory = factory;

        // Should not throw when VisibleDockables is null
        factory.RemoveDockable(new Document(), false);
    }

    [AvaloniaFact]
    public void CleanupOrphanedSplitters_HandlesEmptyVisibleDockables()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;

        // Should not throw when VisibleDockables is empty
        factory.RemoveDockable(new Document(), false);
    }
} 
