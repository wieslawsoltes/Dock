using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using System;
using System.Linq;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Mock dock control for testing Find functionality.
/// </summary>
internal class MockDockControl : IDockControl
{
    public IDockManager DockManager { get; } = null!;
    public IDockControlState DockControlState { get; } = null!;
    public IDock? Layout { get; set; }
    public object? DefaultContext { get; set; }
    public bool InitializeLayout { get; set; }
    public bool InitializeFactory { get; set; }
    public IFactory? Factory { get; set; }
}

/// <summary>
/// Comprehensive unit tests for the RestoreDockable functionality.
/// Tests both the IDockable and string overloads, covering all edge cases
/// and verifying correct splitter management behavior.
/// </summary>
public class FactoryRestoreDockableTests
{
    #region Basic Restoration Tests

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
        doc.Owner = root; // Set the owner so FindRoot can find the root
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        Assert.Null(doc.Owner);
        Assert.Null(doc.OriginalOwner);
        Assert.Empty(root.HiddenDockables!);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesDockableNotInHiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var doc = new Document { Title = "Doc" };
        doc.Owner = root; // Set the owner so FindRoot can find the root
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
        doc.Owner = root; // Set the owner so FindRoot can find the root

        factory.RestoreDockable(doc);
        
        // Should not throw and dockable should remain unchanged
        Assert.Equal(root, doc.Owner);
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

    #endregion

    #region Multiple Dockables Tests

    [AvaloniaFact]
    public void RestoreDockable_RestoresMultipleDockablesToSameOwner()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc1 = new Document { Title = "Doc1", Id = "doc1" };
        var doc2 = new Document { Title = "Doc2", Id = "doc2" };
        
        doc1.OriginalOwner = dock;
        doc1.Owner = root;
        doc2.OriginalOwner = dock;
        doc2.Owner = root;
        
        root.HiddenDockables!.Add(doc1);
        root.HiddenDockables.Add(doc2);

        factory.RestoreDockable(doc1);
        factory.RestoreDockable(doc2);

        Assert.Equal(2, dock.VisibleDockables!.Count);
        Assert.Contains(doc1, dock.VisibleDockables);
        Assert.Contains(doc2, dock.VisibleDockables);
        Assert.Empty(root.HiddenDockables);
    }

    [AvaloniaFact]
    public void RestoreDockable_RestoresToDockWithExistingDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var existingDoc = new Document { Title = "Existing" };
        factory.AddDockable(dock, existingDoc);
        
        var hiddenDoc = new Document { Title = "Hidden", Id = "hidden" };
        hiddenDoc.OriginalOwner = dock;
        hiddenDoc.Owner = root;
        root.HiddenDockables!.Add(hiddenDoc);

        factory.RestoreDockable(hiddenDoc);

        Assert.Equal(2, dock.VisibleDockables!.Count);
        Assert.Contains(existingDoc, dock.VisibleDockables);
        Assert.Contains(hiddenDoc, dock.VisibleDockables);
        Assert.Equal(hiddenDoc, dock.VisibleDockables[1]); // Should be added at the end
    }

    #endregion

    #region Splitter Management Tests

    [AvaloniaFact]
    public void RestoreDockable_HandlesCorrectSplitterManagement_WhenRestoringToEmptyDock()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        // Should handle splitter management correctly for a single dockable
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc, dock.VisibleDockables[0]);
        Assert.All(dock.VisibleDockables, d => Assert.False(d is IProportionalDockSplitter));
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesCorrectSplitterManagement_WhenRestoringToNonEmptyDock()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var existingDoc = new Document { Title = "Existing" };
        factory.AddDockable(dock, existingDoc);
        
        var hiddenDoc = new Document { Title = "Hidden" };
        hiddenDoc.OriginalOwner = dock;
        hiddenDoc.Owner = root;
        root.HiddenDockables!.Add(hiddenDoc);

        factory.RestoreDockable(hiddenDoc);

        // Should handle splitter management correctly when restoring
        Assert.Equal(2, dock.VisibleDockables!.Count);
        Assert.All(dock.VisibleDockables, d => Assert.False(d is IProportionalDockSplitter));
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesCorrectSplitterManagement_WhenDockHasExistingSplitters()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc1 = new Document { Title = "Doc1" };
        var splitter = factory.CreateProportionalDockSplitter();
        var doc2 = new Document { Title = "Doc2" };
        
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, splitter);
        factory.AddDockable(dock, doc2);
        
        var hiddenDoc = new Document { Title = "Hidden" };
        hiddenDoc.OriginalOwner = dock;
        hiddenDoc.Owner = root;
        root.HiddenDockables!.Add(hiddenDoc);

        var initialCount = dock.VisibleDockables!.Count;
        factory.RestoreDockable(hiddenDoc);

        // Should add only the dockable, maintaining existing splitter layout
        Assert.Equal(initialCount + 1, dock.VisibleDockables.Count);
        Assert.Equal(1, dock.VisibleDockables.Count(d => d is IProportionalDockSplitter)); // Only the original splitter
        Assert.Equal(hiddenDoc, dock.VisibleDockables[dock.VisibleDockables.Count - 1]); // Added at the end
    }

    #endregion

    #region String Overload Tests

    [AvaloniaFact]
    public void RestoreDockable_ById_FindsAndRestoresDockable()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc", Id = "test-doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        // Create a mock dock control to make Find work
        var mockDockControl = new MockDockControl { Layout = root };
        factory.DockControls.Add(mockDockControl);

        var result = factory.RestoreDockable("test-doc");

        Assert.Equal(doc, result);
        Assert.Equal(dock, doc.Owner);
        Assert.Single(dock.VisibleDockables!);
        Assert.Empty(root.HiddenDockables);
    }

    [AvaloniaFact]
    public void RestoreDockable_ById_ReturnsNullForNonExistentId()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        // Create a mock dock control to make Find work
        var mockDockControl = new MockDockControl { Layout = root };
        factory.DockControls.Add(mockDockControl);

        var result = factory.RestoreDockable("non-existent-id");

        Assert.Null(result);
    }

    [AvaloniaFact]
    public void RestoreDockable_ById_ReturnsNullForEmptyHiddenDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        // Create a mock dock control to make Find work
        var mockDockControl = new MockDockControl { Layout = root };
        factory.DockControls.Add(mockDockControl);

        var result = factory.RestoreDockable("any-id");

        Assert.Null(result);
    }

    [AvaloniaFact]
    public void RestoreDockable_ById_SearchesMultipleRootDocks()
    {
        var factory = new Factory();
        var root1 = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        var root2 = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root1.Factory = factory;
        root2.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root2, dock);
        
        var doc = new Document { Title = "Doc", Id = "test-doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root2;
        root2.HiddenDockables!.Add(doc);

        // Set up the hierarchy so both roots can be found
        var container = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        container.Factory = factory;
        factory.AddDockable(container, root1);
        factory.AddDockable(container, root2);

        // Create a mock dock control to make Find work  
        var mockDockControl = new MockDockControl { Layout = container };
        factory.DockControls.Add(mockDockControl);

        var result = factory.RestoreDockable("test-doc");

        Assert.Equal(doc, result);
        Assert.Equal(dock, doc.Owner);
    }

    #endregion

    #region Different Dock Types Tests

    [AvaloniaFact]
    public void RestoreDockable_RestoresToToolDock()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var toolDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };
        toolDock.Factory = factory;
        factory.AddDockable(root, toolDock);
        
        var tool = new Tool { Title = "Tool" };
        tool.OriginalOwner = toolDock;
        tool.Owner = root;
        root.HiddenDockables!.Add(tool);

        factory.RestoreDockable(tool);

        Assert.Equal(toolDock, tool.Owner);
        Assert.Single(toolDock.VisibleDockables!);
        Assert.Equal(tool, toolDock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void RestoreDockable_RestoresToDocumentDock()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        docDock.Factory = factory;
        factory.AddDockable(root, docDock);
        
        var doc = new Document { Title = "Document" };
        doc.OriginalOwner = docDock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        Assert.Equal(docDock, doc.Owner);
        Assert.Single(docDock.VisibleDockables!);
        Assert.Equal(doc, docDock.VisibleDockables[0]);
    }

    #endregion

    #region Event Handling Tests

    [AvaloniaFact]
    public void RestoreDockable_FiresOnDockableRestoredEvent()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        IDockable? restoredDockable = null;
        factory.DockableRestored += (sender, args) => restoredDockable = args.Dockable;

        factory.RestoreDockable(doc);

        Assert.Equal(doc, restoredDockable);
    }

    [AvaloniaFact]
    public void RestoreDockable_FiresOnDockableAddedEvent()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        IDockable? addedDockable = null;
        factory.DockableAdded += (sender, args) => addedDockable = args.Dockable;

        factory.RestoreDockable(doc);

        Assert.Equal(doc, addedDockable);
    }

    #endregion

    #region Edge Cases and Error Handling

    [AvaloniaFact]
    public void RestoreDockable_HandlesNullDockableParameter()
    {
        var factory = new Factory();
        
        // Currently RestoreDockable doesn't handle null parameters gracefully
        // This test documents the expected behavior - it should throw NullReferenceException
        Assert.Throws<NullReferenceException>(() => factory.RestoreDockable((IDockable)null!));
    }

    [AvaloniaFact]
    public void RestoreDockable_ById_HandlesNullId()
    {
        var factory = new Factory();
        
        var result = factory.RestoreDockable((string)null!);
        
        Assert.Null(result);
    }

    [AvaloniaFact]
    public void RestoreDockable_ById_HandlesEmptyId()
    {
        var factory = new Factory();
        
        var result = factory.RestoreDockable("");
        
        Assert.Null(result);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesOriginalOwnerWithNullVisibleDockables()
    {
        var factory = new Factory();
        var root = new RootDock { HiddenDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        
        var dock = new ProportionalDock { VisibleDockables = null };
        dock.Factory = factory;
        factory.AddDockable(root, dock);
        
        var doc = new Document { Title = "Doc" };
        doc.OriginalOwner = dock;
        doc.Owner = root;
        root.HiddenDockables!.Add(doc);

        factory.RestoreDockable(doc);

        // Should create VisibleDockables list and add the dockable
        Assert.NotNull(dock.VisibleDockables);
        Assert.Single(dock.VisibleDockables!);
        Assert.Equal(doc, dock.VisibleDockables[0]);
        Assert.Equal(dock, doc.Owner);
    }

    [AvaloniaFact]
    public void RestoreDockable_HandlesNoRootDockFound()
    {
        var factory = new Factory();
        
        var doc = new Document { Title = "Doc" };
        doc.Owner = null; // No way to find root
        
        // Should not throw and should not change anything
        factory.RestoreDockable(doc);
        
        Assert.Null(doc.Owner);
    }

    #endregion
} 