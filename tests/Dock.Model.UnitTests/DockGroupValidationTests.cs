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
        public bool KeepPinnedDockableVisible { get; set; }
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
        public bool EnableGlobalDocking { get; set; } = true;

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
    #pragma warning disable CS0067 // Event never used in tests
    public event EventHandler? CanExecuteChanged;
    #pragma warning restore CS0067
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
                var result = DockGroupValidator.GetEffectiveDockGroup(child);

        Assert.Equal("ChildGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_ShouldInheritFromParent()
    {
        var parent = new SimpleDockable("ParentGroup");
        var child = new SimpleDockable(null) { Owner = parent };

                var result = DockGroupValidator.GetEffectiveDockGroup(child);

        Assert.Equal("ParentGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_ShouldWalkUpHierarchy()
    {
        var grandparent = new SimpleDockable("GrandparentGroup");
        var parent = new SimpleDockable(null) { Owner = grandparent };
        var child = new SimpleDockable(null) { Owner = parent };

                var result = DockGroupValidator.GetEffectiveDockGroup(child);

        Assert.Equal("GrandparentGroup", result);
    }

    [Fact]
    public void GetEffectiveDockGroup_NoGroupInHierarchy_ShouldReturnNull()
    {
        var parent = new SimpleDockable(null);
        var child = new SimpleDockable(null) { Owner = parent };

                var result = DockGroupValidator.GetEffectiveDockGroup(child);

        Assert.Null(result);
    }

    [Fact]
    public void ValidateDockingGroups_SameGroup_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("GroupA");

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_DifferentGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("GroupB");

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_SourceNullGroup_ShouldReject()
    {
        var source = new SimpleDockable(null);
        var target = new SimpleDockable("GroupA");

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_TargetNullGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable(null);

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroups_BothNullGroups_ShouldAllow()
    {
        var source = new SimpleDockable(null);
        var target = new SimpleDockable(null);

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_EmptyStringGroups_ShouldAllow()
    {
        var source = new SimpleDockable("");
        var target = new SimpleDockable("");

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroups_CaseSensitive_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var target = new SimpleDockable("groupa");

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_EmptyDock_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var targetDock = new SimpleDock("GroupB");

                var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_SameGroup_ShouldAllow()
    {
        var source = new SimpleDockable("GroupA");
        var existing = new SimpleDockable("GroupA");
        var targetDock = new SimpleDock("GroupA");
        targetDock.VisibleDockables!.Add(existing);

                var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDockingGroupsInDock_DifferentGroup_ShouldReject()
    {
        var source = new SimpleDockable("GroupA");
        var existing = new SimpleDockable("GroupB");
        var targetDock = new SimpleDock("GroupB");
        targetDock.VisibleDockables!.Add(existing);

                var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.False(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroups_SameGroup_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var target = new SimpleDockable { Id = "Target", DockGroup = "GroupA" };

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.True(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroups_DifferentGroup_ShouldReject()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var target = new SimpleDockable { Id = "Target", DockGroup = "GroupB" };

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroups_BothNullGroups_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = null };
        var target = new SimpleDockable { Id = "Target", DockGroup = null };

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.True(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroups_MixedStates_ShouldReject()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var target = new SimpleDockable { Id = "Target", DockGroup = null };

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.False(result); // Mixed states rejected (grouped can't break out to global)
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_EmptyDock_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var targetDock = new SimpleDock { Id = "TargetDock", VisibleDockables = new List<IDockable>() };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_CompatibleGroup_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var existing = new SimpleDockable { Id = "Existing", DockGroup = "GroupA" };
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = new List<IDockable> { existing } 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_IncompatibleGroup_ShouldReject()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var existing = new SimpleDockable { Id = "Existing", DockGroup = "GroupB" };
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = new List<IDockable> { existing } 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.False(result);
    }

    // ========== COMPREHENSIVE DOCKING GROUP VALIDATION TESTS ==========

    [Theory]
    [InlineData(null, null, true)]     // Rule 1: Non-grouped ↔ Non-grouped = ALLOWED
    [InlineData("", "", true)]         // Rule 1: Empty groups = ALLOWED  
    [InlineData("GroupA", "GroupA", true)]  // Rule 2: Same group = ALLOWED
    [InlineData("GroupA", "GroupB", false)] // Rule 3: Different groups = REJECTED
    [InlineData(null, "GroupA", false)]     // Rule 4: Non-grouped → Grouped = REJECTED (can't contaminate)
    [InlineData("GroupA", null, false)]     // Rule 4: Grouped → Non-grouped = REJECTED (can't break out)
    [InlineData("", "GroupA", false)]       // Rule 4: Empty → Grouped = REJECTED (can't contaminate)
    [InlineData("GroupA", "", false)]       // Rule 4: Grouped → Empty = REJECTED (can't break out)
    public void DockGroupValidator_ValidateDockingGroups_AllScenarios(string? sourceGroup, string? targetGroup, bool expected)
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = sourceGroup };
        var target = new SimpleDockable { Id = "Target", DockGroup = targetGroup };

        var result = DockGroupValidator.ValidateDockingGroups(source, target);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, true)]     // Non-grouped into dock with non-grouped dockable
    [InlineData("", "", true)]         // Empty groups
    [InlineData("GroupA", "GroupA", true)]  // Same group into dock with same group
    [InlineData("GroupA", "GroupB", false)] // Different group into dock with different group
    [InlineData(null, "GroupA", false)]     // Non-grouped into dock with grouped dockable = REJECTED (can't contaminate)
    [InlineData("GroupA", null, false)]     // Grouped into dock with non-grouped dockable = REJECTED (can't break out)
    [InlineData("", "GroupA", false)]       // Empty into dock with grouped dockable = REJECTED (can't contaminate)
    [InlineData("GroupA", "", false)]       // Grouped into dock with empty group dockable = REJECTED (can't break out)
    public void DockGroupValidator_ValidateDockingGroupsInDock_AllScenarios(string? sourceGroup, string? existingGroup, bool expected)
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = sourceGroup };
        var existing = new SimpleDockable { Id = "Existing", DockGroup = existingGroup };
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = new List<IDockable> { existing } 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_NullVisibleDockables_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = null 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result); // Dock with null VisibleDockables should allow any dockable
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_MultipleCompatibleDockables_ShouldAllow()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var existing1 = new SimpleDockable { Id = "Existing1", DockGroup = "GroupA" };
        var existing2 = new SimpleDockable { Id = "Existing2", DockGroup = "GroupA" };
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = new List<IDockable> { existing1, existing2 } 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.True(result);
    }

    [Fact]
    public void DockGroupValidator_ValidateDockingGroupsInDock_OneIncompatibleDockable_ShouldReject()
    {
        var source = new SimpleDockable { Id = "Source", DockGroup = "GroupA" };
        var existing1 = new SimpleDockable { Id = "Existing1", DockGroup = "GroupA" };
        var existing2 = new SimpleDockable { Id = "Existing2", DockGroup = "GroupB" }; // Incompatible
        var targetDock = new SimpleDock 
        { 
            Id = "TargetDock", 
            VisibleDockables = new List<IDockable> { existing1, existing2 } 
        };

        var result = DockGroupValidator.ValidateDockingGroupsInDock(source, targetDock);

        Assert.False(result); // Should reject if ANY existing dockable is incompatible
    }

    [Theory]
    [InlineData(null, null, null)]   // Child null, parent null, grandparent null
    [InlineData(null, "GroupA", null)]   // Child inherits from parent
    [InlineData("GroupB", "GroupA", null)]  // Child group different from parent
    [InlineData(null, null, "GroupC")]   // Child inherits from grandparent
    public void DockGroupValidator_GetEffectiveDockGroup_Inheritance_WorksCorrectly(string? childGroup, string? parentGroup, string? grandparentGroup)
    {
        var grandparent = new SimpleDock { Id = "Grandparent", DockGroup = grandparentGroup };
        var parent = new SimpleDock { Id = "Parent", DockGroup = parentGroup, Owner = grandparent };
        var child = new SimpleDockable { Id = "Child", DockGroup = childGroup, Owner = parent };

        var effectiveGroup = DockGroupValidator.GetEffectiveDockGroup(child);
        var expectedGroup = childGroup ?? parentGroup ?? grandparentGroup;

        Assert.Equal(expectedGroup, effectiveGroup);
    }
}
