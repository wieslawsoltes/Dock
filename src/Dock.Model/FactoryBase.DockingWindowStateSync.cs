// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase : IDockingWindowStateSync
{
    private readonly HashSet<IDockable> _dockingWindowStateCoalescing = new();
    private int _dockingWindowStateSyncDepth;

    private bool IsDockingWindowStateSyncSuppressed => _dockingWindowStateSyncDepth > 0;

    private sealed class DockingWindowStateSyncScope : IDisposable
    {
        private FactoryBase? _factory;

        public DockingWindowStateSyncScope(FactoryBase factory)
        {
            _factory = factory;
            _factory._dockingWindowStateSyncDepth++;
        }

        public void Dispose()
        {
            if (_factory is null)
            {
                return;
            }

            _factory._dockingWindowStateSyncDepth--;
            _factory = null;
        }
    }

    private IDisposable EnterDockingWindowStateSyncScope()
    {
        return new DockingWindowStateSyncScope(this);
    }

    void IDockingWindowStateSync.OnDockingWindowStatePropertyChanged(IDockable dockable, DockingWindowStateProperty property)
    {
        OnDockingWindowStatePropertyChanged(dockable, property);
    }

    /// <summary>
    /// Handles docking-window-state property changes requested by view models.
    /// </summary>
    /// <param name="dockable">The dockable whose state changed.</param>
    /// <param name="property">The changed property.</param>
    protected virtual void OnDockingWindowStatePropertyChanged(IDockable dockable, DockingWindowStateProperty property)
    {
        if (dockable is not IDockingWindowState windowState)
        {
            return;
        }

        if (IsDockingWindowStateSyncSuppressed)
        {
            return;
        }

        if (!_dockingWindowStateCoalescing.Add(dockable))
        {
            return;
        }

        try
        {
            using (EnterDockingWindowStateSyncScope())
            {
                switch (property)
                {
                    case DockingWindowStateProperty.IsOpen:
                        ApplyDockingWindowOpenRequest(dockable, windowState.IsOpen);
                        break;
                    case DockingWindowStateProperty.IsActive:
                        ApplyDockingWindowActiveRequest(dockable, windowState.IsActive);
                        break;
                    case DockingWindowStateProperty.IsSelected:
                        ApplyDockingWindowSelectedRequest(dockable, windowState.IsSelected);
                        break;
                    case DockingWindowStateProperty.DockingState:
                        ApplyDockingWindowStateRequest(dockable, windowState.DockingState);
                        break;
                }
            }

            SynchronizeDockingWindowState(dockable);
        }
        finally
        {
            _dockingWindowStateCoalescing.Remove(dockable);
        }
    }

    private void ApplyDockingWindowOpenRequest(IDockable dockable, bool isOpen)
    {
        var currentlyOpen = IsDockingWindowOpen(dockable);
        if (currentlyOpen == isOpen)
        {
            return;
        }

        if (isOpen)
        {
            if (IsDockableHidden(dockable))
            {
                RestoreDockable(dockable);
            }

            return;
        }

        HideDockable(dockable);
    }

    private void ApplyDockingWindowSelectedRequest(IDockable dockable, bool isSelected)
    {
        if (dockable.Owner is not IDock owner)
        {
            return;
        }

        if (isSelected)
        {
            if (!ReferenceEquals(owner.ActiveDockable, dockable))
            {
                owner.ActiveDockable = dockable;
            }

            return;
        }

        if (ReferenceEquals(owner.ActiveDockable, dockable))
        {
            owner.ActiveDockable = FindFallbackDockable(owner, dockable);
        }
    }

    private void ApplyDockingWindowActiveRequest(IDockable dockable, bool isActive)
    {
        if (dockable.Owner is not IDock owner)
        {
            return;
        }

        var root = FindRoot(owner, x => x.IsFocusableRoot);
        if (root is null)
        {
            return;
        }

        var currentlyActive = ReferenceEquals(root.FocusedDockable, dockable) && owner.IsActive;
        if (currentlyActive == isActive)
        {
            return;
        }

        if (isActive)
        {
            if (!ReferenceEquals(owner.ActiveDockable, dockable) && dockable is not ISplitter)
            {
                owner.ActiveDockable = dockable;
            }

            SetFocusedDockable(owner, dockable);
            return;
        }

        if (ReferenceEquals(root.FocusedDockable, dockable))
        {
            var fallback = owner.ActiveDockable;
            if (ReferenceEquals(fallback, dockable))
            {
                fallback = FindFallbackDockable(owner, dockable);
            }

            SetFocusedDockable(owner, fallback);
        }
    }

    private void ApplyDockingWindowStateRequest(IDockable dockable, DockingWindowState requestedState)
    {
        if (requestedState == DockingWindowState.None)
        {
            return;
        }

        var currentState = ResolveDockingWindowState(dockable);

        var wantsHidden = requestedState.HasFlag(DockingWindowState.Hidden);
        var isHidden = currentState.HasFlag(DockingWindowState.Hidden);

        if (wantsHidden)
        {
            if (!isHidden)
            {
                HideDockable(dockable);
            }

            return;
        }

        if (isHidden)
        {
            RestoreDockable(dockable);
            currentState = ResolveDockingWindowState(dockable);
        }

        var wantsDocument = requestedState.HasFlag(DockingWindowState.Document);
        var wantsPinned = requestedState.HasFlag(DockingWindowState.Pinned);
        var wantsDocked = requestedState.HasFlag(DockingWindowState.Docked);
        var wantsFloating = requestedState.HasFlag(DockingWindowState.Floating);

        if (wantsDocument && !currentState.HasFlag(DockingWindowState.Document))
        {
            DockAsDocument(dockable);
            currentState = ResolveDockingWindowState(dockable);
        }
        else if (wantsPinned && !currentState.HasFlag(DockingWindowState.Pinned))
        {
            PinDockable(dockable);
            currentState = ResolveDockingWindowState(dockable);
        }
        else if (wantsDocked && currentState.HasFlag(DockingWindowState.Pinned))
        {
            UnpinDockable(dockable);
            currentState = ResolveDockingWindowState(dockable);
        }
        else if (wantsDocked && currentState.HasFlag(DockingWindowState.Document))
        {
            DockAsToolIfPossible(dockable);
            currentState = ResolveDockingWindowState(dockable);
        }

        if (wantsFloating && !currentState.HasFlag(DockingWindowState.Floating))
        {
            FloatDockable(dockable);
        }
    }

    private void DockAsToolIfPossible(IDockable dockable)
    {
        if (dockable is not ITool)
        {
            return;
        }

        if (dockable.Owner is not IDock sourceDock)
        {
            return;
        }

        var root = FindRoot(sourceDock, _ => true);
        if (root is null)
        {
            return;
        }

        IDock? targetDock = null;

        if (dockable.OriginalOwner is IDock originalOwner &&
            !ReferenceEquals(originalOwner, sourceDock) &&
            IsOwnerAttached(root, originalOwner))
        {
            targetDock = originalOwner;
        }

        if (targetDock is null)
        {
            targetDock = Find(root, x => x is IToolDock)
                .OfType<IDock>()
                .FirstOrDefault(x =>
                    !ReferenceEquals(x, sourceDock)
                    && !ReferenceEquals(x, root.PinnedDock));
        }

        if (targetDock is null)
        {
            return;
        }

        var targetDockable = targetDock.VisibleDockables?.LastOrDefault();
        MoveDockable(sourceDock, targetDock, dockable, targetDockable);
    }

    private static IEnumerable<IDockable> EnumerateOwnedDockables(IDock dock)
    {
        var seen = new HashSet<IDockable>();

        if (dock.VisibleDockables is not null)
        {
            foreach (var child in dock.VisibleDockables)
            {
                if (seen.Add(child))
                {
                    yield return child;
                }
            }
        }

        if (dock is not ISplitViewDock splitViewDock)
        {
            yield break;
        }

        if (splitViewDock.PaneDockable is { } paneDockable && seen.Add(paneDockable))
        {
            yield return paneDockable;
        }

        if (splitViewDock.ContentDockable is { } contentDockable && seen.Add(contentDockable))
        {
            yield return contentDockable;
        }
    }

    private static IDockable? FindFallbackDockable(IDock dock, IDockable current)
    {
        foreach (var dockable in EnumerateOwnedDockables(dock))
        {
            if (!ReferenceEquals(dockable, current) && dockable is not ISplitter)
            {
                return dockable;
            }
        }

        return null;
    }

    private bool IsDockingWindowOpen(IDockable dockable)
    {
        if (IsDockableHidden(dockable))
        {
            return false;
        }

        if (dockable.Owner is IDock ownerDock)
        {
            if (ownerDock.VisibleDockables?.Contains(dockable) == true)
            {
                return true;
            }

            if (ownerDock is ISplitViewDock splitViewDock)
            {
                if (ReferenceEquals(splitViewDock.PaneDockable, dockable)
                    || ReferenceEquals(splitViewDock.ContentDockable, dockable))
                {
                    return true;
                }
            }
        }

        if (dockable.Owner is not IRootDock rootDock)
        {
            return false;
        }

        return rootDock.LeftPinnedDockables?.Contains(dockable) == true
               || rootDock.RightPinnedDockables?.Contains(dockable) == true
               || rootDock.TopPinnedDockables?.Contains(dockable) == true
               || rootDock.BottomPinnedDockables?.Contains(dockable) == true;
    }

    private bool IsDockingWindowSelected(IDockable dockable)
    {
        if (IsDockableHidden(dockable))
        {
            return false;
        }

        return dockable.Owner is IDock ownerDock && ReferenceEquals(ownerDock.ActiveDockable, dockable);
    }

    private bool IsDockingWindowActive(IDockable dockable)
    {
        if (IsDockableHidden(dockable))
        {
            return false;
        }

        if (dockable.Owner is not IDock ownerDock)
        {
            return false;
        }

        if (!ownerDock.IsActive)
        {
            return false;
        }

        if (FindRoot(dockable, x => x.IsFocusableRoot) is not { } rootDock)
        {
            return false;
        }

        return ReferenceEquals(rootDock.FocusedDockable, dockable);
    }

    private void SetDockingWindowStateFlags(IDockable dockable, DockingWindowState resolvedState)
    {
        if (dockable is not IDockingWindowState windowState)
        {
            return;
        }

        var isOpen = IsDockingWindowOpen(dockable);
        var isSelected = IsDockingWindowSelected(dockable);
        var isActive = IsDockingWindowActive(dockable);

        using (EnterDockingWindowStateSyncScope())
        {
            if (windowState.DockingState != resolvedState)
            {
                windowState.DockingState = resolvedState;
            }

            if (windowState.IsOpen != isOpen)
            {
                windowState.IsOpen = isOpen;
            }

            if (windowState.IsSelected != isSelected)
            {
                windowState.IsSelected = isSelected;
            }

            if (windowState.IsActive != isActive)
            {
                windowState.IsActive = isActive;
            }
        }
    }

    private void SynchronizeDockingWindowSelection(IDock owner)
    {
        foreach (var child in EnumerateOwnedDockables(owner))
        {
            SynchronizeDockingWindowState(child);
        }
    }

    private void SynchronizeDockingWindowState(IDockable? dockable)
    {
        if (dockable is null)
        {
            return;
        }

        var resolvedState = ResolveDockingWindowState(dockable);
        SetDockingWindowStateFlags(dockable, resolvedState);
    }
}
