using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

/// <summary>
/// Unit tests for IsCollapsable functionality in FactoryBase.
/// Tests verify that IsCollapsable=false prevents both empty-dock collapse and single-child simplification.
/// Uses reflection to test the private CleanupProportionalDockTree method.
/// </summary>
public class IsCollapsableTests
{
    private class TestCommand : ICommand
    {
#pragma warning disable CS0067 // Event never used in tests
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }

    private class TestDockable : IDockable
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public object? Context { get; set; }
        public IDockable? Owner { get; set; }
        public IDockable? OriginalOwner { get; set; }
        public IDockable? DefaultDockable { get; set; }
        public IList<IDockable>? VisibleDockables { get; set; }
        public IList<IDockable>? HiddenDockables { get; set; }
        public IList<IDockable>? PinnedDockables { get; set; }
        public IDockable? ActiveDockable { get; set; }
        public IDockable? FocusedDockable { get; set; }
        public double Proportion { get; set; }
        public bool IsActive { get; set; }
        public bool CanClose { get; set; } = true;
        public bool CanPin { get; set; } = true;
        public bool CanFloat { get; set; } = true;
        public bool CanDrag { get; set; } = true;
        public bool CanDrop { get; set; } = true;
        public bool IsModified { get; set; }
        public string? DockGroup { get; set; }
        public IFactory? Factory { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsCollapsable { get; set; } = true;
        public DockMode Dock { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public int ColumnSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public bool IsSharedSizeScope { get; set; }
        public double CollapsedProportion { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; } = double.PositiveInfinity;
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; } = double.PositiveInfinity;

        public bool OnClose() => CanClose;
        public void OnPin() { }
        public void OnSelected() { }
        public bool OnMoveDrag(DockPoint point) => true;
        public bool OnDragEnter(DockPoint point, DragAction dragAction) => true;
        public bool OnDragLeave(DockPoint point) => true;
        public bool OnDragDrop(DockPoint point, DragAction dragAction) => true;
        public void GetVisibleBounds(out double x, out double y, out double width, out double height) { x = y = width = height = 0; }
        public void SetVisibleBounds(double x, double y, double width, double height) { }
        public void OnVisibleBoundsChanged(double x, double y, double width, double height) { }
        public void GetPinnedBounds(out double x, out double y, out double width, out double height) { x = y = width = height = 0; }
        public void SetPinnedBounds(double x, double y, double width, double height) { }
        public void OnPinnedBoundsChanged(double x, double y, double width, double height) { }
        public void GetTabBounds(out double x, out double y, out double width, out double height) { x = y = width = height = 0; }
        public void SetTabBounds(double x, double y, double width, double height) { }
        public void OnTabBoundsChanged(double x, double y, double width, double height) { }
        public void GetPointerPosition(out double x, out double y) { x = y = 0; }
        public void SetPointerPosition(double x, double y) { }
        public void OnPointerPositionChanged(double x, double y) { }
        public void GetPointerScreenPosition(out double x, out double y) { x = y = 0; }
        public void SetPointerScreenPosition(double x, double y) { }
        public void OnPointerScreenPositionChanged(double x, double y) { }
        public string GetControlRecyclingId() => Id;
    }

    private class TestDock : TestDockable, IDock
    {
        public bool CanCloseLastDockable { get; set; } = true;
        public bool CanGoBack { get; } = false;
        public bool CanGoForward { get; } = false;
        public int OpenedDockablesCount { get; set; }
        public ICommand GoBack { get; }
        public ICommand GoForward { get; }
        public ICommand Navigate { get; }
        public ICommand Close { get; }
        public bool EnableGlobalDocking { get; set; } = true;

        public TestDock()
        {
            VisibleDockables = new List<IDockable>();
            GoBack = new TestCommand();
            GoForward = new TestCommand();
            Navigate = new TestCommand();
            Close = new TestCommand();
        }
    }

    private class TestProportionalDock : TestDock, IProportionalDock
    {
        public Orientation Orientation { get; set; }
    }

    private class TestToolDock : TestDock, IToolDock
    {
        public Alignment Alignment { get; set; }
    }

    private class TestDocumentDock : TestDock, IDocumentDock
    {
        public bool CanCreateDocument { get; set; }
        public bool EnableWindowDrag { get; set; } = true;
        public ICommand? CreateDocument { get; set; }
        public DocumentTabLayout TabsLayout { get; set; }
        public void AddDocument(IDockable document) { }
        public void AddTool(IDockable tool) { }
    }

    // Minimal factory implementation for testing - only implements what we need
    private class TestFactory : IFactory
    {
        public IDictionary<IDockable, object> ToolControls => throw new NotImplementedException();
        public IDictionary<IDockable, object> DocumentControls => throw new NotImplementedException();
        public IDictionary<IDockable, IDockableControl> VisibleDockableControls => throw new NotImplementedException();
        public IDictionary<IDockable, object> VisibleRootControls => throw new NotImplementedException();
        public IDictionary<IDockable, IDockableControl> PinnedDockableControls => throw new NotImplementedException();
        public IDictionary<IDockable, object> PinnedRootControls => throw new NotImplementedException();
        public IDictionary<IDockable, IDockableControl> TabDockableControls => throw new NotImplementedException();
        public IDictionary<IDockable, object> TabRootControls => throw new NotImplementedException();
        public IList<IDockControl> DockControls => throw new NotImplementedException();
        public IList<IHostWindow> HostWindows => throw new NotImplementedException();
        public IList<T> CreateList<T>(params T[] items) => throw new NotImplementedException();
        public IRootDock? CreateRootDock() => throw new NotImplementedException();
        public IProportionalDock CreateProportionalDock() => throw new NotImplementedException();
        public IDockDock CreateDockDock() => throw new NotImplementedException();
        public IStackDock CreateStackDock() => throw new NotImplementedException();
        public IGridDock CreateGridDock() => throw new NotImplementedException();
        public IWrapDock CreateWrapDock() => throw new NotImplementedException();
        public IUniformGridDock CreateUniformGridDock() => throw new NotImplementedException();
        public IProportionalDockSplitter CreateProportionalDockSplitter() => throw new NotImplementedException();
        public IGridDockSplitter CreateGridDockSplitter() => throw new NotImplementedException();
        public IToolDock CreateToolDock() => throw new NotImplementedException();
        public IDocumentDock CreateDocumentDock() => throw new NotImplementedException();
        public IDockWindow CreateDockWindow() => throw new NotImplementedException();
        public IDocument CreateDocument() => throw new NotImplementedException();
        public ITool CreateTool() => throw new NotImplementedException();
        public IRootDock? CreateLayout() => throw new NotImplementedException();
        public void InitLayout(IDockable layout) => throw new NotImplementedException();
        public IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation) => throw new NotImplementedException();
        public void SplitToDock(IDock dock, IDockable dockable, DockOperation operation) => throw new NotImplementedException();
        public IDockWindow? CreateWindowFrom(IDockable dockable) => throw new NotImplementedException();
        public void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height) => throw new NotImplementedException();
        public void AddDockable(IDock dock, IDockable dockable) => throw new NotImplementedException();
        public void InsertDockable(IDock dock, IDockable dockable, int index) => throw new NotImplementedException();
        public void AddWindow(IRootDock rootDock, IDockWindow window) => throw new NotImplementedException();
        public void RemoveWindow(IDockWindow window) => throw new NotImplementedException();
        public void SetActiveDockable(IDockable dockable) => throw new NotImplementedException();
        public void SetFocusedDockable(IDock dock, IDockable dockable) => throw new NotImplementedException();
        public IRootDock? FindRoot(IDockable dockable, Func<IRootDock, bool>? predicate = null) => throw new NotImplementedException();
        public void PinDockable(IDockable dockable) => throw new NotImplementedException();
        public bool IsPinned(IDockable dockable) => throw new NotImplementedException();
        public IDock? FindParent(IDock dock, IDockable dockable) => throw new NotImplementedException();
        public void UpdateDockable(IDockable dockable, IDockable? owner) => throw new NotImplementedException();
        public void UpdateWindowsLayout() => throw new NotImplementedException();
        public void CollapseDock(IDock dock) => throw new NotImplementedException();
    }
    
    // Helper method to invoke the private CleanupProportionalDockTree method via reflection
    private static void InvokeCleanupProportionalDockTree(object factory, IProportionalDock dock)
    {
        var factoryBaseType = typeof(FactoryBase);
        var method = factoryBaseType.GetMethod("CleanupProportionalDockTree", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (method != null)
        {
            method.Invoke(factory, new object[] { dock });
        }
    }



    [Fact]
    public void CleanupProportionalDockTree_IsCollapsableFalse_SingleChild_ShouldNotSimplify()
    {
        // Arrange: Create structure where ProportionalDock with IsCollapsable=false has only one child
        
        // Root ProportionalDock (simulating MainArea from the issue)
        var mainArea = new TestProportionalDock 
        { 
            Id = "MainArea", 
            IsCollapsable = false,
            Orientation = Orientation.Vertical
        };

        // Child DocumentDock
        var documentDock = new TestDocumentDock 
        { 
            Id = "MainDocuments",
            Owner = mainArea
        };

        mainArea.VisibleDockables!.Add(documentDock);

        // Owner of mainArea
        var rootOwner = new TestProportionalDock { Id = "Root" };
        rootOwner.VisibleDockables!.Add(mainArea);
        mainArea.Owner = rootOwner;

        // Act: Directly call CleanupProportionalDockTree on mainArea using reflection
        InvokeCleanupProportionalDockTree(new TestFactory(), mainArea);

        // Assert: MainArea should still exist in rootOwner even though it has only one child
        Assert.Contains(mainArea, rootOwner.VisibleDockables);
        Assert.Single(mainArea.VisibleDockables);
        Assert.Equal(documentDock, mainArea.VisibleDockables[0]);
    }

    [Fact]
    public void CleanupProportionalDockTree_IsCollapsableTrue_SingleChild_ShouldSimplify()
    {
        // Arrange: Create structure where ProportionalDock with IsCollapsable=true has only one child
        
        var proportionalDock = new TestProportionalDock 
        { 
            Id = "SimplifiableDock", 
            IsCollapsable = true,  // Default behavior - should simplify
            Orientation = Orientation.Vertical
        };

        var documentDock = new TestDocumentDock 
        { 
            Id = "Document",
            Owner = proportionalDock
        };

        proportionalDock.VisibleDockables!.Add(documentDock);

        var rootOwner = new TestProportionalDock { Id = "Root" };
        rootOwner.VisibleDockables!.Add(proportionalDock);
        proportionalDock.Owner = rootOwner;

        // Act: Directly call CleanupProportionalDockTree on proportionalDock using reflection
        InvokeCleanupProportionalDockTree(new TestFactory(), proportionalDock);

        // Assert: ProportionalDock should be simplified away and documentDock should be in root
        Assert.DoesNotContain(proportionalDock, rootOwner.VisibleDockables);
        Assert.Contains(documentDock, rootOwner.VisibleDockables);
        Assert.Equal(rootOwner, documentDock.Owner);
    }

    [Fact]
    public void NestedProportionalDocks_MixedIsCollapsable_BehavesCorrectly()
    {
        // Arrange: Nested structure with mixed IsCollapsable settings

        var root = new TestProportionalDock { Id = "Root" };
        
        var outer = new TestProportionalDock
        {
            Id = "Outer",
            IsCollapsable = false,
            Owner = root
        };
        root.VisibleDockables!.Add(outer);

        var inner = new TestProportionalDock
        {
            Id = "Inner",
            IsCollapsable = true,
            Owner = outer
        };
        outer.VisibleDockables!.Add(inner);

        var leaf = new TestDocumentDock
        {
            Id = "Leaf",
            Owner = inner
        };
        inner.VisibleDockables!.Add(leaf);

        // Act: Call cleanup on inner (which should simplify it), then on outer (which should NOT simplify)
        InvokeCleanupProportionalDockTree(new TestFactory(), inner);
        InvokeCleanupProportionalDockTree(new TestFactory(), outer);

        // Assert: 
        // - Inner should be simplified away (IsCollapsable=true)
        // - Outer should remain (IsCollapsable=false) even with one child
        Assert.Contains(outer, root.VisibleDockables);
        Assert.DoesNotContain(inner, outer.VisibleDockables);
        Assert.Contains(leaf, outer.VisibleDockables);
        Assert.Equal(outer, leaf.Owner);
    }
}
