// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <summary>
    /// Updates the docking window state for the specified dockable.
    /// </summary>
    /// <param name="dockable">The dockable to update.</param>
    protected virtual void UpdateDockingWindowState(IDockable dockable)
    {
        var resolvedState = ResolveDockingWindowState(dockable);
        using (EnterDockingWindowStateSyncScope())
        {
            dockable.DockingState = resolvedState;
        }

        SetDockingWindowStateFlags(dockable, resolvedState);
    }

    /// <summary>
    /// Updates the docking window state for the specified dockable and all nested dockables.
    /// </summary>
    /// <param name="dockable">The root dockable to update.</param>
    protected virtual void UpdateDockingWindowStateRecursive(IDockable dockable)
    {
        UpdateDockingWindowState(dockable);

        if (dockable is not IDock dock)
        {
            return;
        }

        if (dock.VisibleDockables is not null)
        {
            foreach (var child in dock.VisibleDockables)
            {
                UpdateDockingWindowStateRecursive(child);
            }
        }

        if (dockable is not ISplitViewDock splitViewDock)
        {
            return;
        }

        var paneDockable = splitViewDock.PaneDockable;
        if (paneDockable is not null && dock.VisibleDockables?.Contains(paneDockable) != true)
        {
            UpdateDockingWindowStateRecursive(paneDockable);
        }

        var contentDockable = splitViewDock.ContentDockable;
        if (contentDockable is not null &&
            !ReferenceEquals(contentDockable, paneDockable) &&
            dock.VisibleDockables?.Contains(contentDockable) != true)
        {
            UpdateDockingWindowStateRecursive(contentDockable);
        }
    }

    /// <summary>
    /// Resolves the docking window state for the specified dockable.
    /// </summary>
    /// <param name="dockable">The dockable to resolve.</param>
    /// <returns>The resolved docking window state.</returns>
    protected virtual DockingWindowState ResolveDockingWindowState(IDockable dockable)
    {
        var state = ResolveDockingWindowLocationState(dockable);

        if (IsDockableInFloatingArea(dockable))
        {
            state |= DockingWindowState.Floating;
        }

        if (IsDockableHidden(dockable))
        {
            state |= DockingWindowState.Hidden;
        }

        return state;
    }

    private DockingWindowState ResolveDockingWindowLocationState(IDockable dockable)
    {
        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock is not null && IsDockablePinned(dockable, rootDock))
        {
            return DockingWindowState.Pinned;
        }

        return IsDockableInDocumentArea(dockable)
            ? DockingWindowState.Document
            : DockingWindowState.Docked;
    }

    private bool IsDockableHidden(IDockable dockable)
    {
        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock?.HiddenDockables is null)
        {
            return false;
        }

        if (rootDock.HiddenDockables.Contains(dockable))
        {
            return true;
        }

        for (IDockable? current = dockable.Owner;
             current is not null && !ReferenceEquals(current, rootDock);
             current = current.Owner)
        {
            if (rootDock.HiddenDockables.Contains(current))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDockableInFloatingArea(IDockable dockable)
    {
        if (IsDockableInFloatingAreaCore(dockable))
        {
            return true;
        }

        return dockable.OriginalOwner is { } originalOwner && IsDockableInFloatingAreaCore(originalOwner);
    }

    private static bool IsDockableInFloatingAreaCore(IDockable dockable)
    {
        var rootDock = FindOwnerRoot(dockable);
        return rootDock?.Window?.Owner is IRootDock;
    }

    private static IRootDock? FindOwnerRoot(IDockable dockable)
    {
        for (IDockable? current = dockable; current is not null; current = current.Owner)
        {
            if (current is IRootDock rootDock)
            {
                return rootDock;
            }
        }

        return null;
    }

    private static bool IsDockableInDocumentArea(IDockable dockable)
    {
        if (IsDockableInDocumentAreaCore(dockable))
        {
            return true;
        }

        return dockable.OriginalOwner is { } originalOwner && IsDockableInDocumentAreaCore(originalOwner);
    }

    private static bool IsDockableInDocumentAreaCore(IDockable dockable)
    {
        for (IDockable? current = dockable; current is not null; current = current.Owner)
        {
            if (current is IDocumentDock)
            {
                return true;
            }
        }

        return false;
    }
}
