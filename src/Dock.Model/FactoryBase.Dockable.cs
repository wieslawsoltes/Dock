// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void AddDockable(IDock dock, IDockable dockable)
    {
        InitDockable(dockable, dock);
        dock.VisibleDockables ??= CreateList<IDockable>();
        AddVisibleDockable(dock, dockable);
        OnDockableAdded(dockable);
    }

    /// <inheritdoc/>
    public virtual void InsertDockable(IDock dock, IDockable dockable, int index)
    {
        if (index >= 0)
        {
            InitDockable(dockable, dock);
            dock.VisibleDockables ??= CreateList<IDockable>();
            InsertVisibleDockable(dock, index, dockable);
            OnDockableAdded(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveDockable(IDockable dockable, bool collapse)
    {
        // to correctly remove a pinned dockable, it needs to be unpinned
        UnpinDockable(dockable);

        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        var index = dock.VisibleDockables.IndexOf(dockable);
        if (index < 0)
        {
            return;
        }

        var wasActive = dock.ActiveDockable == dockable;

        RemoveVisibleDockable(dock, dockable);
        OnDockableRemoved(dockable);

        if (wasActive)
        {
            var indexActiveDockable = index > 0 ? index - 1 : 0;
            if (dock.VisibleDockables.Count > 0)
            {
                var nextActiveDockable = dock.VisibleDockables[indexActiveDockable];
                dock.ActiveDockable = nextActiveDockable is not ISplitter ? nextActiveDockable : null;
            }
            else
            {
                dock.ActiveDockable = null;
            }
        }

        // Clean up orphaned splitters
        CleanupOrphanedSplitters(dock);

        if (collapse)
        {
            CollapseDock(dock);
        }
    }

    /// <summary>
    /// Cleans up orphaned splitters that are no longer needed.
    /// A splitter is orphaned if:
    /// 1. It's at the beginning or end of the dockables list
    /// 2. It's consecutive with another splitter
    /// 3. It's the only dockable in the list
    /// A splitter between two non-splitter dockables is NOT orphaned and should be kept.
    /// </summary>
    protected void CleanupOrphanedSplitters(IDock dock)
    {
        if (dock.VisibleDockables is null || dock.VisibleDockables.Count == 0)
            return;

        bool needsCleanup = true;
        
        // Repeat cleanup until no more orphaned splitters are found
        // This handles cases where removing splitters creates new orphaned splitters
        while (needsCleanup)
        {
            needsCleanup = false;
            var toRemove = new List<IDockable>();
            var dockables = dock.VisibleDockables.ToList();

            // If we only have splitters, remove all of them
            if (dockables.Count > 0 && dockables.All(d => d is ISplitter))
            {
                toRemove.AddRange(dockables);
            }
            else
            {
                // Remove splitters that are at the beginning
                while (dockables.Count > 0 && dockables[0] is ISplitter)
                {
                    toRemove.Add(dockables[0]);
                    dockables.RemoveAt(0);
                }

                // Remove splitters that are at the end
                while (dockables.Count > 0 && dockables[dockables.Count - 1] is ISplitter)
                {
                    toRemove.Add(dockables[dockables.Count - 1]);
                    dockables.RemoveAt(dockables.Count - 1);
                }

                // Remove consecutive splitters - keep only the first one in each sequence
                for (int i = 0; i < dockables.Count - 1; i++)
                {
                    if (dockables[i] is ISplitter)
                    {
                        // Remove all consecutive splitters after this one
                        int j = i + 1;
                        while (j < dockables.Count && dockables[j] is ISplitter)
                        {
                            toRemove.Add(dockables[j]);
                            j++;
                        }
                    }
                }
            }

            // Remove the identified splitters using direct removal to avoid recursion
            foreach (var splitter in toRemove)
            {
                if (dock.VisibleDockables.Contains(splitter))
                {
                    RemoveVisibleDockable(dock, splitter);
                    OnDockableRemoved(splitter);
                    needsCleanup = true; // Check again after removal
                }
            }
        }

        // Update active dockable if it was a splitter that got removed
        if (dock.ActiveDockable is ISplitter || 
            (dock.ActiveDockable is not null && !dock.VisibleDockables.Contains(dock.ActiveDockable)))
        {
            dock.ActiveDockable = dock.VisibleDockables?.FirstOrDefault(d => d is not ISplitter);
        }
    }

    /// <inheritdoc/>
    public virtual void MoveDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            RemoveVisibleDockableAt(dock, sourceIndex);
            OnDockableRemoved(sourceDockable);
            OnDockableUndocked(sourceDockable, DockOperation.Fill);
            InsertVisibleDockable(dock, targetIndex, sourceDockable);
            OnDockableAdded(sourceDockable);
            OnDockableMoved(sourceDockable);
            OnDockableDocked(sourceDockable, DockOperation.Fill);
            dock.ActiveDockable = sourceDockable;

            // Clean up orphaned splitters that might have been created by the move
            CleanupOrphanedSplitters(dock);
        }
    }

    /// <inheritdoc/>
    public virtual void MovePinnedDockable(IDockable sourceDockable, IDockable targetDockable)
    {
        var rootDock = FindRoot(sourceDockable, _ => true);
        if (rootDock is null)
        {
            return;
        }

        // Find which pinned collection contains both dockables
        var pinnedCollection = GetPinnedCollection(sourceDockable, rootDock);
        if (pinnedCollection is null || !pinnedCollection.Contains(targetDockable))
        {
            return; // Source and target must be in the same pinned collection
        }

        var sourceIndex = pinnedCollection.IndexOf(sourceDockable);
        var targetIndex = pinnedCollection.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            pinnedCollection.RemoveAt(sourceIndex);
            pinnedCollection.Insert(targetIndex, sourceDockable);
            OnDockableMoved(sourceDockable);
        }
    }

    private IList<IDockable>? GetPinnedCollection(IDockable dockable, IRootDock rootDock)
    {
        if (rootDock.LeftPinnedDockables?.Contains(dockable) == true)
        {
            return rootDock.LeftPinnedDockables;
        }

        if (rootDock.RightPinnedDockables?.Contains(dockable) == true)
        {
            return rootDock.RightPinnedDockables;
        }

        if (rootDock.TopPinnedDockables?.Contains(dockable) == true)
        {
            return rootDock.TopPinnedDockables;
        }

        if (rootDock.BottomPinnedDockables?.Contains(dockable) == true)
        {
            return rootDock.BottomPinnedDockables;
        }

        return null;
    }

    /// <inheritdoc/>
    public virtual void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        UnpinDockable(sourceDockable);

        if (targetDock.VisibleDockables is null)
        {
            targetDock.VisibleDockables = CreateList<IDockable>();
            if (targetDock.VisibleDockables is null)
            {
                return;
            }
        }

        var isSameOwner = sourceDock == targetDock;

        var targetIndex = 0;

        if (sourceDock.VisibleDockables is not null && targetDock.VisibleDockables is not null && targetDock.VisibleDockables.Count > 0)
        {
            if (isSameOwner)
            {
                var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);

                if (targetDockable is not null)
                {
                    targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);
                }
                else
                {
                    targetIndex = targetDock.VisibleDockables.Count - 1;
                }

                if (sourceIndex == targetIndex)
                {
                    return;
                }
            }
            else
            {
                if (targetDockable is not null)
                {
                    targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);
                    if (targetIndex >= 0)
                    {
                        targetIndex += 1;
                    }
                    else
                    {
                        targetIndex = targetDock.VisibleDockables.Count - 1;
                    }
                }
                else
                {
                    targetIndex = targetDock.VisibleDockables.Count - 1;
                }
            }
        }

        if (sourceDock.VisibleDockables is not null && targetDock.VisibleDockables is not null)
        {
            if (isSameOwner)
            {
                var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);
                if (sourceIndex < targetIndex)
                {
                    InsertVisibleDockable(targetDock, targetIndex + 1, sourceDockable);
                    OnDockableAdded(sourceDockable);
                    RemoveVisibleDockableAt(targetDock, sourceIndex);
                    OnDockableRemoved(sourceDockable);
                    OnDockableUndocked(sourceDockable, DockOperation.Fill);
                    OnDockableMoved(sourceDockable);
                    OnDockableDocked(sourceDockable, DockOperation.Fill);
                }
                else
                {
                    var removeIndex = sourceIndex + 1;
                    if (targetDock.VisibleDockables.Count + 1 > removeIndex)
                    {
                        InsertVisibleDockable(targetDock, targetIndex, sourceDockable);
                        OnDockableAdded(sourceDockable);
                        RemoveVisibleDockableAt(targetDock, removeIndex);
                        OnDockableRemoved(sourceDockable);
                        OnDockableUndocked(sourceDockable, DockOperation.Fill);
                        OnDockableMoved(sourceDockable);
                        OnDockableDocked(sourceDockable, DockOperation.Fill);
                    }
                }

                // Clean up orphaned splitters in the dock where the move occurred
                CleanupOrphanedSplitters(targetDock);
            }
            else
            {
                RemoveDockable(sourceDockable, true);
                OnDockableUndocked(sourceDockable, DockOperation.Fill);
                InsertVisibleDockable(targetDock, targetIndex, sourceDockable);
                
                InitDockable(sourceDockable, targetDock);
                targetDock.ActiveDockable = sourceDockable;
                
                OnDockableAdded(sourceDockable);
                OnDockableMoved(sourceDockable);
                OnDockableDocked(sourceDockable, DockOperation.Fill);

                // Clean up orphaned splitters in both source and target docks
                // Note: sourceDock cleanup is already handled by RemoveDockable call above
                CleanupOrphanedSplitters(targetDock);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void SwapDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            var originalSourceDockable = dock.VisibleDockables[sourceIndex];
            var originalTargetDockable = dock.VisibleDockables[targetIndex];

            dock.VisibleDockables[targetIndex] = originalSourceDockable;
            OnDockableRemoved(originalTargetDockable);
            OnDockableAdded(originalSourceDockable);
            dock.VisibleDockables[sourceIndex] = originalTargetDockable;
            OnDockableAdded(originalTargetDockable);
            OnDockableSwapped(originalSourceDockable);
            OnDockableSwapped(originalTargetDockable);
            dock.ActiveDockable = originalTargetDockable;

            // Clean up orphaned splitters that might have been created by the swap
            CleanupOrphanedSplitters(dock);
        }
    }

    /// <inheritdoc/>
    public virtual void SwapDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (sourceDock.VisibleDockables is null || targetDock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0)
        {
            var originalSourceDockable = sourceDock.VisibleDockables[sourceIndex];
            var originalTargetDockable = targetDock.VisibleDockables[targetIndex];
            sourceDock.VisibleDockables[sourceIndex] = originalTargetDockable;
            targetDock.VisibleDockables[targetIndex] = originalSourceDockable;

            InitDockable(originalSourceDockable, targetDock);
            InitDockable(originalTargetDockable, sourceDock);

            OnDockableSwapped(originalTargetDockable);
            OnDockableSwapped(originalSourceDockable);

            sourceDock.ActiveDockable = originalTargetDockable;
            targetDock.ActiveDockable = originalSourceDockable;

            // Clean up orphaned splitters in both docks
            CleanupOrphanedSplitters(sourceDock);
            CleanupOrphanedSplitters(targetDock);
        }
    }

    /// <inheritdoc/>
    public bool IsDockablePinned(IDockable dockable, IRootDock? rootDock = null)
    {
        if (rootDock == null)
        {
            rootDock = FindRoot(dockable);

            if (rootDock == null)
            {
                return false;
            }
        }

        if (rootDock.LeftPinnedDockables is not null)
        {
            if (rootDock.LeftPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.RightPinnedDockables is not null)
        {
            if (rootDock.RightPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.TopPinnedDockables is not null)
        {
            if (rootDock.TopPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.BottomPinnedDockables is not null)
        {
            if (rootDock.BottomPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public void HidePreviewingDockables(IRootDock rootDock)
    {
        HidePreviewingDockablesInternal(rootDock, respectKeepVisible: true);
    }

    private void HidePreviewingDockablesInternal(IRootDock rootDock, bool respectKeepVisible)
    {
        if (rootDock.PinnedDock?.VisibleDockables is null)
        {
            return;
        }

        var dockables = rootDock.PinnedDock.VisibleDockables.ToList();
        foreach (var dockable in dockables)
        {
            if (respectKeepVisible && dockable.KeepPinnedDockableVisible)
            {
                continue;
            }

            dockable.Owner = dockable.OriginalOwner;
            dockable.OriginalOwner = null;
            RemoveVisibleDockable(rootDock.PinnedDock, dockable);
        }

        if (rootDock.PinnedDock.VisibleDockables?.Count == 0)
        {
            rootDock.PinnedDock = null;
        }
    }

    /// <inheritdoc/>
    public void PreviewPinnedDockable(IDockable dockable)
    {
        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock is null)
        {
            return;
        }

        HidePreviewingDockablesInternal(rootDock, respectKeepVisible: false);

        var owner = dockable.Owner;
        
        // For pinned dockables, determine alignment based on which pinned collection they belong to
        var alignment = GetPinnedDockableAlignment(dockable, rootDock);
        
        // If not found in pinned collections, fall back to owner alignment
        if (alignment == Alignment.Unset)
        {
            alignment = (owner as IToolDock)?.Alignment ?? Alignment.Unset;
        }

        rootDock.PinnedDock ??= CreateToolDock();
        rootDock.PinnedDock.Alignment = alignment;
        
        // Disable dropping into the preview dock since it's only for preview purposes
        rootDock.PinnedDock.CanDrop = false;

        RemoveAllVisibleDockables(rootDock.PinnedDock);

        if (rootDock.PinnedDock.VisibleDockables?.Contains(dockable) != true)
        {
            if (dockable.OriginalOwner is null && owner is IToolDock)
            {
                // Only set OriginalOwner if the dockable comes from a tool dock.
                // For dockables pinned as part of the initial layout (owner is typically the root),
                // keep OriginalOwner null so that unpin actions are correctly gated/disabled by the UI.
                dockable.OriginalOwner = owner;
            }
            AddVisibleDockable(rootDock.PinnedDock!, dockable);
        }
 
        InitDockable(rootDock.PinnedDock, rootDock);
    }

    private Alignment GetPinnedDockableAlignment(IDockable dockable, IRootDock rootDock)
    {
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

    private void UpdatePinnedBoundsFromVisible(IDockable dockable, IDock owner)
    {
        dockable.GetVisibleBounds(out _, out _, out var width, out var height);

        if (!IsValidSize(width) || !IsValidSize(height))
        {
            owner.GetVisibleBounds(out _, out _, out width, out height);
        }

        if (!IsValidSize(width) || !IsValidSize(height))
        {
            return;
        }

        dockable.SetPinnedBounds(0, 0, width, height);
    }

    private static bool IsValidSize(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }

    /// <inheritdoc/>
    public virtual void PinDockable(IDockable dockable)
    {
        switch (dockable.Owner)
        {
            case IToolDock toolDock:
            {
                var rootDock = FindRoot(dockable, _ => true);
                if (rootDock is null)
                {
                    return;
                }

                var isVisible = false;

                if (toolDock.VisibleDockables is not null)
                {
                    isVisible = toolDock.VisibleDockables.Contains(dockable);
                }

                var isPinned = IsDockablePinned(dockable, rootDock);

                var originalToolDock = dockable.OriginalOwner as IToolDock;

                var alignment = originalToolDock?.Alignment ?? toolDock.Alignment;

                if (isVisible && !isPinned)
                {
                    // Pin dockable.
                    UpdatePinnedBoundsFromVisible(dockable, toolDock);

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            rootDock.LeftPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Right:
                        {
                            rootDock.RightPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Top:
                        {
                            rootDock.TopPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Bottom:
                        {
                            rootDock.BottomPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                    }

                    if (toolDock.VisibleDockables is not null)
                    {
                        RemoveVisibleDockable(toolDock, dockable);
                        OnDockableRemoved(dockable);
                    }

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            if (rootDock.LeftPinnedDockables is not null)
                            {
                                rootDock.LeftPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Right:
                        {
                            if (rootDock.RightPinnedDockables is not null)
                            {
                                rootDock.RightPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Top:
                        {
                            if (rootDock.TopPinnedDockables is not null)
                            {
                                rootDock.TopPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Bottom:
                        {
                            if (rootDock.BottomPinnedDockables is not null)
                            {
                                rootDock.BottomPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                    }

                    // TODO: Handle ActiveDockable state.
                    // TODO: Handle IsExpanded property of IToolDock.
                    // TODO: Handle AutoHide property of IToolDock.
                }
                else if (isPinned)
                {
                    // Unpin dockable.

                    toolDock.VisibleDockables ??= CreateList<IDockable>();

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            if (rootDock.LeftPinnedDockables is not null)
                            {
                                rootDock.LeftPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Right:
                        {
                            if (rootDock.RightPinnedDockables is not null)
                            {
                                rootDock.RightPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Top:
                        {
                            if (rootDock.TopPinnedDockables is not null)
                            {
                                rootDock.TopPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Bottom:
                        {
                            if (rootDock.BottomPinnedDockables is not null)
                            {
                                rootDock.BottomPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                    }

                    if (!isVisible)
                    {
                        // Not currently visible in the preview tool dock; add back to it.
                        AddVisibleDockable(toolDock, dockable);
                        OnDockableAdded(dockable);
                        InitDockable(dockable, toolDock);
                        toolDock.ActiveDockable = dockable;
                    }
                    else
                    {
                        // Visible in preview dock; close preview and restore into appropriate owner.
                        // Prefer explicit original owner if available; otherwise, find or create a suitable
                        // tool dock matching the alignment and insert it into the layout.

                        var targetOwner = dockable.OriginalOwner as IDock;

                        // Close preview and reset Owner/OriginalOwner for all previewed items.
                        HidePreviewingDockablesInternal(rootDock, respectKeepVisible: false);

                        if (targetOwner is null)
                        {
                            // Try to find an existing tool dock with the same alignment.
                            var targetToolDock = FindToolDockByAlignment(rootDock, alignment);
                            if (targetToolDock is null)
                            {
                                // Create and insert a new tool dock on the requested side.
                                targetToolDock = CreateToolDock();
                                targetToolDock.Title = nameof(IToolDock);
                                targetToolDock.Alignment = alignment;
                                targetToolDock.VisibleDockables = CreateList<IDockable>();

                                // Choose an anchor dock to split next to.
                                var anchorDock = GetPreferredAnchorDock(rootDock) ?? rootDock.ActiveDockable as IDock;
                                if (anchorDock is not null)
                                {
                                    var op = alignment switch
                                    {
                                        Alignment.Left => DockOperation.Left,
                                        Alignment.Right => DockOperation.Right,
                                        Alignment.Top => DockOperation.Top,
                                        Alignment.Bottom => DockOperation.Bottom,
                                        _ => DockOperation.Left
                                    };
                                    SplitToDock(anchorDock, targetToolDock, op);
                                }
                                else
                                {
                                    // As a last resort, attach directly to root if empty structure.
                                    if (rootDock.VisibleDockables is null)
                                    {
                                        rootDock.VisibleDockables = CreateList<IDockable>();
                                    }
                                    AddVisibleDockable(rootDock, targetToolDock);
                                    OnDockableAdded(targetToolDock);
                                    InitDockable(targetToolDock, rootDock);
                                }
                            }

                            targetOwner = targetToolDock;
                        }

                        AddVisibleDockable(targetOwner, dockable);
                        OnDockableAdded(dockable);
                        InitDockable(dockable, targetOwner);
                        if (targetOwner is IDock targetDock)
                        {
                            targetDock.ActiveDockable = dockable;
                        }
                    }

                    // TODO: Handle ActiveDockable state.
                    // TODO: Handle IsExpanded property of IToolDock.
                    // TODO: Handle AutoHide property of IToolDock.
                }
                else
                {
                    // TODO: Handle invalid state.
                }

                break;
            }
        }
    }

    private IToolDock? FindToolDockByAlignment(IRootDock root, Alignment alignment)
    {
        foreach (var d in Find(root, x => x is IToolDock td && td.Alignment == alignment))
        {
            if (d is IToolDock td)
                return td;
        }
        return null;
    }

    private IDock? GetPreferredAnchorDock(IRootDock root)
    {
        // Prefer the first visible dockable that is a dock and not the PinnedDock
        var result = Find(root, x => x is IDock && x != root.PinnedDock);
        foreach (var item in result)
        {
            return (IDock)item;
        }
        return null;
    }

    /// <inheritdoc/>
    public void UnpinDockable(IDockable dockable)
    {
        if (IsDockablePinned(dockable))
        {
            PinDockable(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void FloatDockable(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock)
        {
            return;
        }

        UnpinDockable(dockable);

        dock.GetPointerScreenPosition(out var dockPointerScreenX, out var dockPointerScreenY);
        dockable.GetPointerScreenPosition(out var dockablePointerScreenX, out var dockablePointerScreenY);

        if (double.IsNaN(dockablePointerScreenX))
        {
            dockablePointerScreenX = dockPointerScreenX;
        }
        if (double.IsNaN(dockablePointerScreenY))
        {
            dockablePointerScreenY = dockPointerScreenY;
        }

        dock.GetVisibleBounds(out var ownerX, out var ownerY, out var ownerWidth, out var ownerHeight);
        dockable.GetVisibleBounds(out var dockableX, out var dockableY, out var dockableWidth, out var dockableHeight);

        if (double.IsNaN(dockablePointerScreenX))
        {
            dockablePointerScreenX = !double.IsNaN(dockableX) ? dockableX : !double.IsNaN(ownerX) ? ownerX : 0;
        }
        if (double.IsNaN(dockablePointerScreenY))
        {
            dockablePointerScreenY = !double.IsNaN(dockableY) ? dockableY : !double.IsNaN(ownerY) ? ownerY : 0;
        }
        if (double.IsNaN(dockableWidth))
        {
            dockableWidth = double.IsNaN(ownerWidth) ? 300 : ownerWidth;
        }
        if (double.IsNaN(dockableHeight))
        {
            dockableHeight = double.IsNaN(ownerHeight) ? 400 : ownerHeight;
        }

        SplitToWindow(dock, dockable, dockablePointerScreenX, dockablePointerScreenY, dockableWidth, dockableHeight);
    }

    /// <inheritdoc/>
    public virtual void FloatAllDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is null)
        {
            return;
        }

        dock.GetPointerScreenPosition(out var pointerX, out var pointerY);
        dock.GetVisibleBounds(out var ownerX, out var ownerY, out var ownerWidth, out var ownerHeight);

        if (double.IsNaN(pointerX))
        {
            pointerX = !double.IsNaN(ownerX) ? ownerX : 0;
        }
        if (double.IsNaN(pointerY))
        {
            pointerY = !double.IsNaN(ownerY) ? ownerY : 0;
        }

        var width = double.IsNaN(ownerWidth) ? 300 : ownerWidth;
        var height = double.IsNaN(ownerHeight) ? 400 : ownerHeight;

        IDock targetDock = dock switch
        {
            IDocumentDock => CreateDocumentDock(),
            IToolDock => CreateToolDock(),
            _ => CreateDockDock()
        };

        targetDock.Title = dock.Title;
        targetDock.Id = dock.Id;
        targetDock.VisibleDockables = CreateList<IDockable>();

        if (dock is IDocumentDock sourceDoc && targetDock is IDocumentDock targetDoc)
        {
            targetDoc.CanCreateDocument = sourceDoc.CanCreateDocument;
            targetDoc.EnableWindowDrag = sourceDoc.EnableWindowDrag;

            if (sourceDoc is IDocumentDockContent sourceContent && targetDoc is IDocumentDockContent targetContent)
            {
                targetContent.DocumentTemplate = sourceContent.DocumentTemplate;
            }
        }

        if (dock is IToolDock sourceTool && targetDock is IToolDock targetTool)
        {
            targetTool.Alignment = sourceTool.Alignment;
            targetTool.IsExpanded = sourceTool.IsExpanded;
            targetTool.AutoHide = sourceTool.AutoHide;
            targetTool.GripMode = sourceTool.GripMode;
        }

        var dockables = dock.VisibleDockables.ToList();
        foreach (var d in dockables)
        {
            // move dockables one by one but do not force collapse when the
            // owner dock declares it should stay visible
            RemoveDockable(d, dock.IsCollapsable);
            AddDockable(targetDock, d);
            OnDockableMoved(d);
            targetDock.ActiveDockable = d;
        }

        var window = CreateWindowFrom(targetDock);
        if (window is not null)
        {
            AddWindow(rootDock, window);
            window.X = pointerX;
            window.Y = pointerY;
            window.Width = width;
            window.Height = height;
            window.Present(false);

            if (window.Layout is { })
            {
                SetFocusedDockable(window.Layout, targetDock.ActiveDockable);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void DockAsDocument(IDockable dockable)
    {
        if (dockable.Owner is not IDock sourceDock)
        {
            return;
        }

        var rootDock = FindRoot(sourceDock, _ => true);
        if (rootDock is null)
        {
            return;
        }

        var target = FindDockable(rootDock, d => d is IDocumentDock) as IDock;
        if (target is null)
        {
            return;
        }

        var targetDockable = target.VisibleDockables?.LastOrDefault();

        MoveDockable(sourceDock, target, dockable, targetDockable);
    }

    /// <inheritdoc/>
    public virtual void CloseDockable(IDockable dockable)
    {
        if (dockable.Owner is IDock dock &&
            !dock.CanCloseLastDockable &&
            dock.VisibleDockables?.Count <= 1)
        {
            return;
        }

        if (dockable.CanClose && OnDockableClosing(dockable))
        {
            // Check if this document was generated from ItemsSource and remove the source item
            HandleItemsSourceDocumentClosing(dockable);

            var hide = (dockable is ITool && HideToolsOnClose)
                       || (dockable is IDocument && HideDocumentsOnClose);

            if (hide)
            {
                HideDockable(dockable);
            }
            else
            {
                RemoveDockable(dockable, true);
            }

            OnDockableClosed(dockable);
        }
    }

    /// <summary>
    /// Handles closing of documents that were generated from ItemsSource by removing the source item from the collection.
    /// </summary>
    /// <param name="dockable">The dockable being closed.</param>
    protected virtual void HandleItemsSourceDocumentClosing(IDockable dockable)
    {
        // Only handle documents
        if (dockable is not IDocument document)
            return;

        // Check if the owner dock supports ItemsSource
        if (dockable.Owner is IItemsSourceDock itemsSourceDock)
        {
            // Check if this document was generated from ItemsSource
            if (itemsSourceDock.IsDocumentFromItemsSource(dockable))
            {
                // Get the source item from the document's Context
                var sourceItem = document.Context;
                if (sourceItem != null)
                {
                    // Remove the source item from the ItemsSource collection
                    itemsSourceDock.RemoveItemFromSource(sourceItem);
                }
            }
        }
    }

    private void CloseDockablesRange(IDock dock, int start, int end, IDockable? excluding = null)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        for (var i = end; i >= start; --i)
        {
            if (excluding == null || dock.VisibleDockables[i] != excluding)
            {
                CloseDockable(dock.VisibleDockables[i]);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void CloseOtherDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        CloseDockablesRange(dock, 0, dock.VisibleDockables.Count - 1, dockable);
    }

    /// <inheritdoc/>
    public virtual void CloseAllDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        CloseDockablesRange(dock, 0, dock.VisibleDockables.Count - 1);
    }

    /// <inheritdoc/>
    public virtual void CloseLeftDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        int indexOf = dock.VisibleDockables.IndexOf(dockable);
        if (indexOf == -1)
        {
            return;
        }

        CloseDockablesRange(dock, 0, indexOf - 1);
    }

    /// <inheritdoc/>
    public virtual void CloseRightDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        int indexOf = dock.VisibleDockables.IndexOf(dockable);
        if (indexOf == -1)
        {
            return;
        }

        CloseDockablesRange(dock, indexOf + 1, dock.VisibleDockables.Count - 1);
    }

    /// <inheritdoc/>
    public virtual void NewHorizontalDocumentDock(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock)
        {
            return;
        }

        var newDock = CreateDocumentDock();
        newDock.Title = nameof(IDocumentDock);
        newDock.VisibleDockables = CreateList<IDockable>();

        if (dock is IDocumentDock sourceDock && newDock is IDocumentDock targetDock)
        {
            targetDock.Id = sourceDock.Id;
            targetDock.CanCreateDocument = sourceDock.CanCreateDocument;
            targetDock.EnableWindowDrag = sourceDock.EnableWindowDrag;
            targetDock.LayoutMode = sourceDock.LayoutMode;

            if (sourceDock is IDocumentDockContent sdc && targetDock is IDocumentDockContent tdc)
            {
                tdc.DocumentTemplate = sdc.DocumentTemplate;
            }
        }

        MoveDockable(dock, newDock, dockable, null);
        SplitToDock(dock, newDock, DockOperation.Right);
    }

    /// <inheritdoc/>
    public virtual void SetDocumentDockTabsLayout(IDockable dockable, DocumentTabLayout layout)
    {
        if (dockable is IDocumentDock documentDock)
        {
            documentDock.TabsLayout = layout;
        }
    }

    /// <inheritdoc/>
    public void SetDocumentDockTabsLayoutLeft(IDockable dockable) => SetDocumentDockTabsLayout(dockable, DocumentTabLayout.Left);

    /// <inheritdoc/>
    public void SetDocumentDockTabsLayoutTop(IDockable dockable) => SetDocumentDockTabsLayout(dockable, DocumentTabLayout.Top);

    /// <inheritdoc/>
    public void SetDocumentDockTabsLayoutRight(IDockable dockable) => SetDocumentDockTabsLayout(dockable, DocumentTabLayout.Right);

    /// <inheritdoc/>
    public virtual void SetDocumentDockLayoutMode(IDockable dockable, DocumentLayoutMode layoutMode)
    {
        if (dockable is IDocumentDock documentDock)
        {
            documentDock.LayoutMode = layoutMode;
        }
    }

    /// <inheritdoc/>
    public void SetDocumentDockLayoutModeTabbed(IDockable dockable) => SetDocumentDockLayoutMode(dockable, DocumentLayoutMode.Tabbed);

    /// <inheritdoc/>
    public void SetDocumentDockLayoutModeMdi(IDockable dockable) => SetDocumentDockLayoutMode(dockable, DocumentLayoutMode.Mdi);
    
    /// <inheritdoc/>
    public virtual void NewVerticalDocumentDock(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock)
        {
            return;
        }

        var newDock = CreateDocumentDock();
        newDock.Title = nameof(IDocumentDock);
        newDock.VisibleDockables = CreateList<IDockable>();

        if (dock is IDocumentDock sourceDock && newDock is IDocumentDock targetDock)
        {
            targetDock.Id = sourceDock.Id;
            targetDock.CanCreateDocument = sourceDock.CanCreateDocument;
            targetDock.EnableWindowDrag = sourceDock.EnableWindowDrag;
            targetDock.LayoutMode = sourceDock.LayoutMode;

            if (sourceDock is IDocumentDockContent sdc && targetDock is IDocumentDockContent tdc)
            {
                tdc.DocumentTemplate = sdc.DocumentTemplate;
            }
        }

        MoveDockable(dock, newDock, dockable, null);
        SplitToDock(dock, newDock, DockOperation.Bottom);
    }

    /// <inheritdoc/>
    public virtual void HideDockable(IDockable dockable)
    {
        UnpinDockable(dockable);

        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock is null)
        {
            return;
        }

        rootDock.HiddenDockables ??= CreateList<IDockable>();

        if (dockable.Owner is IDock owner && owner.VisibleDockables is not null)
        {
            RemoveVisibleDockable(owner, dockable);
            OnDockableRemoved(dockable);
            dockable.OriginalOwner = owner;

            // Clean up orphaned splitters that might have been created by hiding the dockable
            CleanupOrphanedSplitters(owner);
        }

        dockable.Owner = rootDock;
        rootDock.HiddenDockables.Add(dockable);
        OnDockableHidden(dockable);
    }

    /// <inheritdoc/>
    public void HideDockable(string id)
    {
        var dockable = Find(d => d.Id == id).FirstOrDefault();
        if (dockable is not null)
        {
            HideDockable(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void RestoreDockable(IDockable dockable)
    {
        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock?.HiddenDockables is null)
        {
            return;
        }

        if (!rootDock.HiddenDockables.Contains(dockable))
        {
            return;
        }

        rootDock.HiddenDockables.Remove(dockable);
        OnDockableRestored(dockable);

        if (dockable.OriginalOwner is IDock owner)
        {
            AddVisibleDockable(owner, dockable);
            OnDockableAdded(dockable);
            dockable.Owner = owner;
            dockable.OriginalOwner = null;
        }
        else
        {
            dockable.Owner = null;
        }
    }

    /// <inheritdoc/>
    public IDockable? RestoreDockable(string id)
    {
        foreach (var rootDock in Find(d => d is IRootDock).OfType<IRootDock>())
        {
            if (rootDock.HiddenDockables is null)
            {
                continue;
            }

            var dockable = rootDock.HiddenDockables.FirstOrDefault(x => x.Id == id);
            if (dockable is not null)
            {
                RestoreDockable(dockable);
                return dockable;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds the dockable to the visible dockables list of the dock.
    /// </summary>
    protected void AddVisibleDockable(IDock dock, IDockable dockable)
    {
        if (dock.VisibleDockables == null)
        {
            dock.VisibleDockables = CreateList<IDockable>();
        }
        dock.VisibleDockables.Add(dockable);
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Inserts the dockable to the visible dockables list of the dock at the specified index.
    /// </summary>
    protected void InsertVisibleDockable(IDock dock, int index, IDockable dockable)
    {
        if (dock.VisibleDockables == null)
        {
            dock.VisibleDockables = CreateList<IDockable>();
        }
        dock.VisibleDockables.Insert(index, dockable);
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Removes the dockable from the visible dockables list of the dock.
    /// </summary>
    protected void RemoveVisibleDockable(IDock dock, IDockable dockable)
    {
        if (dock.VisibleDockables != null)
        {
            dock.VisibleDockables.Remove(dockable);
        }
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Removes all visible dockable of the dock.
    /// </summary>
    protected void RemoveAllVisibleDockables(IDock dock)
    {
        if (dock.VisibleDockables != null)
        {
            if (dock.VisibleDockables.Count > 0)
            {
                dock.VisibleDockables.Clear();
            }
        }
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Removes the dockable at the specified index from the visible dockables list of the dock.
    /// </summary>
    protected void RemoveVisibleDockableAt(IDock dock, int index)
    {
        if (dock.VisibleDockables != null)
        {
            dock.VisibleDockables.RemoveAt(index);
        }

        UpdateIsEmpty(dock);
    }

    private static bool IsDockableEmpty(IDockable? dockable)
    {
        return dockable is null
               || dockable is ISplitter
               || dockable is IDock { IsEmpty: true, IsCollapsable: true };
    }

    private static bool IsSplitViewDockEmpty(ISplitViewDock splitViewDock)
    {
        var visibleDockables = splitViewDock.VisibleDockables;
        if (visibleDockables is { Count: > 0 } &&
            visibleDockables.Any(dockable => !IsDockableEmpty(dockable)))
        {
            return false;
        }

        return IsSplitViewDockContentEmpty(splitViewDock);
    }

    private static bool IsSplitViewDockContentEmpty(ISplitViewDock splitViewDock)
    {
        return IsDockableEmpty(splitViewDock.PaneDockable)
               && IsDockableEmpty(splitViewDock.ContentDockable);
    }

    private void UpdateIsEmpty(IDock dock)
    {
        bool oldIsEmpty = dock.IsEmpty;
        var visibleDockables = dock.VisibleDockables;

        var newIsEmpty = dock switch
        {
            ISplitViewDock splitViewDock => IsSplitViewDockEmpty(splitViewDock),
            _ => visibleDockables == null
                 || visibleDockables.Count == 0
                 || visibleDockables.All(IsDockableEmpty)
        };

        if (oldIsEmpty != newIsEmpty)
        {
            dock.IsEmpty = newIsEmpty;
            if (dock.Owner is IDock parent)
            {
                UpdateIsEmpty(parent);
            }
        }

        UpdateOpenedDockablesCount(dock);
    }

    private void UpdateOpenedDockablesCount(IDockable dockable)
    {
        switch (dockable)
        {
            case IProportionalDock proportionalDock:
                proportionalDock.OpenedDockablesCount = proportionalDock.VisibleDockables?.Sum(x => (x as IDock)?.OpenedDockablesCount ?? 0) ?? 0;
                break;
            case IRootDock rootDock:
                rootDock.OpenedDockablesCount = rootDock.VisibleDockables?.Sum(x => (x as IDock)?.OpenedDockablesCount ?? 0) ?? 0;
                break;
            case IDock dock:
                dock.OpenedDockablesCount = 1;
                break;
            default:
                break;
        }

        if (dockable.Owner != null)
        {
            UpdateOpenedDockablesCount(dockable.Owner);
        }
    }
}
