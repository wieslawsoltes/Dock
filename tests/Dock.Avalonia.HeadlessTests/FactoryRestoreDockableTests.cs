using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryRestoreDockableTests
{
    [AvaloniaFact]
    public void RestoreDockable_AddsDockableToOriginalOwner()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        // Add the dock to the root so it can be found
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root; // Set the owner so FindRoot can find the root
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        // The RestoreDockable method does set the owner to the original owner and adds it back
        Assert.Equal(dock, doc.Owner);
        Assert.Null(doc.OriginalOwner);
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc, dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesNullOriginalOwner()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = null;
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        Assert.Null(doc.Owner);
        Assert.Null(doc.OriginalOwner);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesDockableNotInHiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var doc = new Document { Title = "Doc" };
        // Don't add the dockable to HiddenDockables

        factory.RestoreDockable(doc);

        Assert.Empty(root.HiddenDockables!);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesNullHiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = null };
        root.Factory = factory;
        
        var doc = new Document { Title = "Doc" };

        factory.RestoreDockable(doc);
    }

    [AvaloniaFact]
    public void RestoreDockable_RemovesFromHiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        
        // Add the dock to the root so it can be found
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root; // Set the owner so FindRoot can find the root
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        // The RestoreDockable method does remove from HiddenDockables
        Assert.Empty(root.HiddenDockables!);
    }
} 