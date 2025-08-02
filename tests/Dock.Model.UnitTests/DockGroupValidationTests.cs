using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

/// <summary>
/// Unit tests for docking group validation in DockService.
/// Tests focus on the core validation logic using reflection to access private methods.
/// </summary>
public class DockGroupValidationTests
{
    private class SimpleDockable : IDockable
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
        public bool IsCollapsable { get; set; }
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

        public SimpleDockable(string? dockGroup = null) 
        { 
            DockGroup = dockGroup; 
            VisibleDockables = new List<IDockable>();
        }

        // Implement required methods with minimal functionality
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

    private class SimpleDock : SimpleDockable, IDock
    {
        public bool CanCloseLastDockable { get; set; } = true;
        public bool CanGoBack { get; } = false;
        public bool CanGoForward { get; } = false;
        public int OpenedDockablesCount { get; set; }
        public ICommand GoBack { get; }
        public ICommand GoForward { get; }
        public ICommand Navigate { get; }
        public ICommand Close { get; }

        public SimpleDock(string? dockGroup = null) : base(dockGroup) 
        { 
            VisibleDockables = new List<IDockable>();
            GoBack = new TestCommand();
            GoForward = new TestCommand();
            Navigate = new TestCommand();
            Close = new TestCommand();
        }
    }

    private class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }

    private class TestDockService : IDockService
    {
        public void Close(IDockable dockable) { }
        public void SetActiveDockable(IDockable? dockable) { }
        public void SetFocusedDockable(IDock dock, IDockable dockable) { }
        public void ShowWindows() { }
        public void ExitWindows() { }
        public bool MoveDockable(IDockable sourceDockable, IDock sourceOwner, IDock targetOwner, bool bExecute) => true;
        public bool SwapDockable(IDockable sourceDockable, IDock sourceOwner, IDock targetOwner, bool bExecute) => true;
        public bool SplitDockable(IDockable targetDockable, IDock sourceOwner, IDock targetOwner, DockOperation operation, bool bExecute) => true;
        public bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DockPoint point, bool bExecute) => true;
        public bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction dragAction, bool bExecute) => true;
    }

    [Fact]
    public void GetEffectiveDockGroup_ShouldReturnOwnGroup()
    {
        var parent = new SimpleDockable("ParentGroup");
        var child = new SimpleDockable("ChildGroup") { Owner = parent };

        // Use reflection to test the private method
        var method = typeof(DockService).GetMethod("GetEffectiveDockGroup", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string?)method?.Invoke(null, new object[] { child });

        Assert.Equal("ChildGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_ShouldInheritFromParent()
    {
        var parent = new SimpleDockable("ParentGroup");
        var child = new SimpleDockable(null) { Owner = parent };

        var method = typeof(DockService).GetMethod("GetEffectiveDockGroup", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string?)method?.Invoke(null, new object[] { child });

        Assert.Equal("ParentGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_ShouldWalkUpHierarchy()
    {
        var grandparent = new SimpleDockable("GrandparentGroup");
        var parent = new SimpleDockable(null) { Owner = grandparent };
        var child = new SimpleDockable(null) { Owner = parent };

        var method = typeof(DockService).GetMethod("GetEffectiveDockGroup", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string?)method?.Invoke(null, new object[] { child });

        Assert.Equal("GrandparentGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_NoGroupInHierarchy_ShouldReturnNull()
    {
        var parent = new SimpleDockable(null);
        var child = new SimpleDockable(null) { Owner = parent };

        var method = typeof(DockService).GetMethod("GetEffectiveDockGroup", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (string?)method?.Invoke(null, new object[] { child });

        Assert.Null(result);
    }

    [Fact]
    public void ValidateDockingGroups_SameGroup_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("GroupA");

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_DifferentGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("GroupB");

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_SourceNullGroup_ShouldReject()
    {
        var source = new SimpleDockable(null);
        var target = new SimpleDockable("GroupA");

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_TargetNullGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable(null);

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_BothNullGroups_ShouldAllow()
    {
        var source = new SimpleDockable(null);
        var target = new SimpleDockable(null);

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_EmptyStringGroups_ShouldAllow()
    {
        var source = new SimpleDockable("");
        var target = new SimpleDockable("");

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_CaseSensitive_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("groupa");

        var method = typeof(DockService).GetMethod("ValidateDockingGroups", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, target })!;

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_EmptyDock_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var targetDock = new SimpleDock("GroupB");

        var method = typeof(DockService).GetMethod("ValidateDockingGroupsInDock", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, targetDock })!;

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_SameGroup_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var existing = new SimpleDockable("GroupA");
        var targetDock = new SimpleDock("GroupA");
        targetDock.VisibleDockables!.Add(existing);

        var method = typeof(DockService).GetMethod("ValidateDockingGroupsInDock", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, targetDock })!;

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_DifferentGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var existing = new SimpleDockable("GroupB");
        var targetDock = new SimpleDock("GroupB");
        targetDock.VisibleDockables!.Add(existing);

        var method = typeof(DockService).GetMethod("ValidateDockingGroupsInDock", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method?.Invoke(null, new object[] { source, targetDock })!;

        Assert.False(result);
    }
}
