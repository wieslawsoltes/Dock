// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Docking manager.
/// </summary>
public class DockManager : IDockManager
{
    private readonly IDockService _dockService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockManager"/> class.
    /// </summary>
    /// <param name="dockService">The dock service.</param>
    public DockManager(IDockService dockService)
    {
        _dockService = dockService;
    }

    /// <inheritdoc/>
    public DockPoint Position { get; set; }

    /// <inheritdoc/>
    public DockPoint ScreenPosition { get; set; }

    /// <inheritdoc/>
    public bool PreventSizeConflicts { get; set; } = true;

    private static bool IsFixed(double min, double max)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return !double.IsNaN(min) && !double.IsNaN(max) && min == max;
    }

    private static bool HasSizeConflict(ITool a, ITool b)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var widthConflict = IsFixed(a.MinWidth, a.MaxWidth) && IsFixed(b.MinWidth, b.MaxWidth) && a.MinWidth != b.MinWidth;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var heightConflict = IsFixed(a.MinHeight, a.MaxHeight) && IsFixed(b.MinHeight, b.MaxHeight) && a.MinHeight != b.MinHeight;
        return widthConflict || heightConflict;
    }

    /// <summary>
    /// Validates if two dockables can be docked together based on their docking groups.
    /// Both source and target must be in the same "group state":
    /// - Both have null/empty groups (unrestricted docking)
    /// - Both have the same non-null group (restricted docking within group)
    /// Mixed states (one grouped, one ungrouped) are rejected to prevent contamination.
    /// This method considers group inheritance through the dock hierarchy.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDockable">The target dockable or dock.</param>
    /// <returns>True if the docking groups are compatible; otherwise false.</returns>
    private static bool ValidateDockingGroups(IDockable sourceDockable, IDockable targetDockable)
    {
        var sourceGroup = GetEffectiveDockGroup(sourceDockable);
        var targetGroup = GetEffectiveDockGroup(targetDockable);

        // Both must be in the same "group state"
        var sourceHasGroup = !string.IsNullOrEmpty(sourceGroup);
        var targetHasGroup = !string.IsNullOrEmpty(targetGroup);

        // If both have no group, allow unrestricted docking
        if (!sourceHasGroup && !targetHasGroup)
        {
            return true;
        }

        // If both have groups, they must match exactly
        if (sourceHasGroup && targetHasGroup)
        {
            return string.Equals(sourceGroup, targetGroup, StringComparison.Ordinal);
        }

        // Mixed states (one grouped, one ungrouped) are not allowed
        return false;
    }

    /// <summary>
    /// Gets the effective docking group for a dockable, considering inheritance.
    /// If the dockable has its own group, that is used. Otherwise, walks up the
    /// ownership hierarchy to find an inherited group.
    /// </summary>
    /// <param name="dockable">The dockable to get the effective group for.</param>
    /// <returns>The effective docking group, or null if none is found.</returns>
    private static string? GetEffectiveDockGroup(IDockable dockable)
    {
        var current = dockable;
        
        // Walk up the hierarchy until we find a group or reach the root
        while (current != null)
        {
            if (!string.IsNullOrEmpty(current.DockGroup))
            {
                return current.DockGroup;
            }
            
            current = current.Owner;
        }
        
        return null;
    }

    /// <summary>
    /// Validates if a dockable can be docked into a dock based on docking groups.
    /// The source must be compatible with all existing dockables in the target dock.
    /// This method considers group inheritance through the dock hierarchy.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDock">The target dock.</param>
    /// <returns>True if the docking groups are compatible; otherwise false.</returns>
    private static bool ValidateDockingGroupsInDock(IDockable sourceDockable, IDock targetDock)
    {
        // If the target dock has no visible dockables, allow the operation
        if (targetDock.VisibleDockables == null || targetDock.VisibleDockables.Count == 0)
        {
            return true;
        }

        // Check compatibility with all existing dockables in the target dock
        foreach (var existingDockable in targetDock.VisibleDockables)
        {
            if (!ValidateDockingGroups(sourceDockable, existingDockable))
            {
                return false;
            }
        }

        return true;
    }

    private bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
    {
        return operation switch
        {
            DockOperation.Fill => _dockService.MoveDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
            DockOperation.Left => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Right => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Top => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Bottom => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Window => _dockService.DockDockableIntoWindow(sourceDockable, targetDock, ScreenPosition, bExecute),
            _ => false
        };
    }

    private bool DockDockableIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDockable.Owner is not IDock sourceDockableOwner)
        {
            return false;
        }

        return DockDockableIntoDock(sourceDockable, sourceDockableOwner, targetDock, action, operation, bExecute);
    }

    private bool DockDockableIntoDock(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        return action switch
        {
            DragAction.Copy => false,
            DragAction.Move => DockDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DragAction.Link => _dockService.SwapDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
            _ => false
        };
    }

    private bool DockDockableIntoDockVisible(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        var visible = sourceDock.VisibleDockables?.ToList();
        if (visible is null)
        {
            return true;
        }

        foreach (var dockable in visible)
        {
            if (DockDockableIntoDock(dockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool DockDockIntoDock(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        var visible = sourceDock.VisibleDockables?.ToList();
        if (visible is null)
        {
            return true;
        }
            
        if (visible.Count == 1)
        {
            var sourceDockable = visible.FirstOrDefault();
            if (sourceDockable is null || DockDockableIntoDock(sourceDockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
        }
        else
        {
            var sourceDockable = visible.FirstOrDefault();
            if (sourceDockable is null || DockDockableIntoDock(sourceDockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
                
            foreach (var dockable in visible.Skip(1))
            {
                var targetDockable = visible.FirstOrDefault();
                if (targetDockable is null || _dockService.DockDockableIntoDockable(dockable, targetDockable, action, bExecute) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool DockDockable(IDock sourceDock, IDockable targetDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        return operation switch
        {
            DockOperation.Fill => DockDockableIntoDockVisible(sourceDock, targetDock, action, operation, bExecute),
            DockOperation.Window => _dockService.DockDockableIntoWindow(sourceDock, targetDockable, ScreenPosition, bExecute),
            _ => DockDockIntoDock(sourceDock, targetDock, action, operation, bExecute)
        };
    }

    /// <inheritdoc/>
    public bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceTool.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        // Check docking groups compatibility
        if (targetDockable is IDock targetDock)
        {
            if (!ValidateDockingGroupsInDock(sourceTool, targetDock))
            {
                return false;
            }
        }
        else if (!ValidateDockingGroups(sourceTool, targetDockable))
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceTool, targetDockable, ScreenPosition, bExecute),
            IToolDock toolDock =>
                (!PreventSizeConflicts || toolDock.VisibleDockables?.OfType<ITool>().All(t => !HasSizeConflict(sourceTool, t)) != false)
                && DockDockableIntoDock(sourceTool, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceTool, documentDock, action, operation, bExecute),
            ITool tool => ValidateToolToTool(sourceTool, tool, action, bExecute),
            IDocument document => _dockService.DockDockableIntoDockable(sourceTool, document, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(sourceTool, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    private bool ValidateToolToTool(ITool sourceTool, ITool targetTool, DragAction action, bool bExecute)
    {
        if (PreventSizeConflicts && HasSizeConflict(sourceTool, targetTool))
        {
            return false;
        }

        // Check if target tool is pinned - if so, pin the source tool instead of docking normally
        if (targetTool.Owner?.Factory is { } factory)
        {
            if (factory.IsDockablePinned(targetTool))
            {
                var targetAlignment = GetPinnedDockableAlignment(targetTool, factory);
                var sourceAlignment = GetPinnedDockableAlignment(sourceTool, factory);
                
                // Check if source is also pinned
                if (factory.IsDockablePinned(sourceTool))
                {
                    // Same alignment - prevent normal docking, reordering should be handled by ItemDragHelper
                    if (sourceAlignment == targetAlignment)
                    {
                        return false;
                    }
                    
                    // Move from one pinned side to another
                    if (bExecute)
                    {
                        MovePinnedDockable(sourceTool, sourceAlignment, targetAlignment, factory);
                    }
                    return true;
                }
                else
                {
                    // Set source tool alignment to match target's pin alignment before pinning
                    if (bExecute)
                    {
                        SetToolAlignment(sourceTool, targetAlignment);
                        factory.PinDockable(sourceTool);
                    }
                    return true;
                }
            }
        }

        return _dockService.DockDockableIntoDockable(sourceTool, targetTool, action, bExecute);
    }

    private static Alignment GetPinnedDockableAlignment(IDockable dockable, IFactory factory)
    {
        var rootDock = factory.FindRoot(dockable, _ => true);
        if (rootDock is null)
        {
            return Alignment.Unset;
        }

        if (rootDock.LeftPinnedDockables?.Contains(dockable) == true)
        {
            return Alignment.Left;
        }

        if (rootDock.RightPinnedDockables?.Contains(dockable) == true)
        {
            return Alignment.Right;
        }

        if (rootDock.TopPinnedDockables?.Contains(dockable) == true)
        {
            return Alignment.Top;
        }

        if (rootDock.BottomPinnedDockables?.Contains(dockable) == true)
        {
            return Alignment.Bottom;
        }

        return Alignment.Unset;
    }

    private static void SetToolAlignment(ITool tool, Alignment alignment)
    {
        if (tool.Owner is IToolDock toolDock)
        {
            toolDock.Alignment = alignment;
        }
    }

    private static void MovePinnedDockable(IDockable dockable, Alignment sourceAlignment, Alignment targetAlignment, IFactory factory)
    {
        var rootDock = factory.FindRoot(dockable, _ => true);
        if (rootDock is null)
        {
            return;
        }

        // Remove from source pinned collection
        RemoveFromPinnedCollection(dockable, sourceAlignment, rootDock, factory);

        // Add to target pinned collection
        AddToPinnedCollection(dockable, targetAlignment, rootDock, factory);
    }

    private static void RemoveFromPinnedCollection(IDockable dockable, Alignment alignment, IRootDock rootDock, IFactory factory)
    {
        switch (alignment)
        {
            case Alignment.Left:
                rootDock.LeftPinnedDockables?.Remove(dockable);
                break;
            case Alignment.Right:
                rootDock.RightPinnedDockables?.Remove(dockable);
                break;
            case Alignment.Top:
                rootDock.TopPinnedDockables?.Remove(dockable);
                break;
            case Alignment.Bottom:
                rootDock.BottomPinnedDockables?.Remove(dockable);
                break;
        }
        
        // Note: We don't call OnDockableUnpinned here since we're moving, not unpinning
    }

    private static void AddToPinnedCollection(IDockable dockable, Alignment alignment, IRootDock rootDock, IFactory factory)
    {
        switch (alignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                rootDock.LeftPinnedDockables ??= factory.CreateList<IDockable>();
                rootDock.LeftPinnedDockables?.Add(dockable);
                break;
            case Alignment.Right:
                rootDock.RightPinnedDockables ??= factory.CreateList<IDockable>();
                rootDock.RightPinnedDockables?.Add(dockable);
                break;
            case Alignment.Top:
                rootDock.TopPinnedDockables ??= factory.CreateList<IDockable>();
                rootDock.TopPinnedDockables?.Add(dockable);
                break;
            case Alignment.Bottom:
                rootDock.BottomPinnedDockables ??= factory.CreateList<IDockable>();
                rootDock.BottomPinnedDockables?.Add(dockable);
                break;
        }
        
        // Fire moved event since we moved the dockable between pinned collections
        if (factory is FactoryBase factoryBase)
        {
            factoryBase.OnDockableMoved(dockable);
        }
    }

    /// <inheritdoc/>
    public bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceDocument.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        // Check docking groups compatibility
        if (targetDockable is IDock targetDock)
        {
            if (!ValidateDockingGroupsInDock(sourceDocument, targetDock))
            {
                return false;
            }
        }
        else if (!ValidateDockingGroups(sourceDocument, targetDockable))
        {
            return false;
        }

        if (targetDockable is ITool or IToolDock)
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceDocument, targetDockable, ScreenPosition, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceDocument, documentDock, action, operation, bExecute),
            IDocument document => _dockService.DockDockableIntoDockable(sourceDocument, document, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(sourceDocument, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceDock.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        // Check docking groups compatibility
        if (targetDockable is IDock targetDock)
        {
            if (!ValidateDockingGroupsInDock(sourceDock, targetDock))
            {
                return false;
            }
        }
        else if (!ValidateDockingGroups(sourceDock, targetDockable))
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceDock, targetDockable, ScreenPosition, bExecute),
            IToolDock toolDock => 
                sourceDock != toolDock && DockDockable(sourceDock, targetDockable, toolDock, action, operation, bExecute),
            ITool { Owner: IToolDock toolDock } => 
                sourceDock != toolDock && DockDockable(sourceDock, targetDockable, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => 
                sourceDock != documentDock && DockDockable(sourceDock, targetDockable, documentDock, action, operation, bExecute),
            IDocument { Owner: IDocumentDock documentDock } => 
                sourceDock != documentDock && DockDockable(sourceDock, targetDockable, documentDock, action, operation, bExecute),
            IProportionalDock proportionalDock => 
                sourceDock != proportionalDock && DockDockable(sourceDock, targetDockable, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    private bool ValidateProportionalDock(IProportionalDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateStackDock(IStackDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateGridDock(IGridDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateWrapDock(IWrapDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateUniformGridDock(IUniformGridDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    /// <inheritdoc/>
    public bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        return sourceDockable switch
        {
            IToolDock toolDock => ValidateDock(toolDock, targetDockable, action, operation, bExecute),
            IDocumentDock documentDock => ValidateDock(documentDock, targetDockable, action, operation, bExecute),
            ITool tool => ValidateTool(tool, targetDockable, action, operation, bExecute),
            IDocument document => ValidateDocument(document, targetDockable, action, operation, bExecute),
            IProportionalDock proportionalDock => ValidateProportionalDock(proportionalDock, targetDockable, action, operation, bExecute),
            IStackDock stackDock => ValidateStackDock(stackDock, targetDockable, action, operation, bExecute),
            IGridDock gridDock => ValidateGridDock(gridDock, targetDockable, action, operation, bExecute),
            IWrapDock wrapDock => ValidateWrapDock(wrapDock, targetDockable, action, operation, bExecute),
            IUniformGridDock uniformGridDock => ValidateUniformGridDock(uniformGridDock, targetDockable, action, operation, bExecute),
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool IsDockTargetVisible(IDockable sourceDockable, IDockable targetDockable, DockOperation operation)
    {
        if (operation != DockOperation.Fill)
        {
            return true;
        }

        if (ReferenceEquals(sourceDockable, targetDockable))
        {
            return false;
        }

        if (ReferenceEquals(sourceDockable.Owner, targetDockable))
        {
            return false;
        }

        return true;
    }
}
