// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    private GlobalDockTrackingState _globalDockTrackingState = GlobalDockTrackingState.Empty;

    /// <inheritdoc />
    public GlobalDockTrackingState GlobalDockTrackingState => _globalDockTrackingState;

    /// <inheritdoc />
    public IDockable? CurrentDockable => _globalDockTrackingState.Dockable;

    /// <inheritdoc />
    public IRootDock? CurrentRootDock => _globalDockTrackingState.RootDock;

    /// <inheritdoc />
    public IDockWindow? CurrentDockWindow => _globalDockTrackingState.Window;

    /// <inheritdoc />
    public IHostWindow? CurrentHostWindow => _globalDockTrackingState.HostWindow;

    private void InitializeGlobalDockTracking(IRootDock rootDock)
    {
        var dockable = ResolveTrackedDockable(rootDock);
        var resolvedRoot = ResolveRootDock(dockable) ?? rootDock;
        UpdateGlobalDockTracking(
            dockable,
            resolvedRoot,
            resolvedRoot.Window,
            DockTrackingChangeReason.LayoutInitialized);
    }

    private static IDockable? ResolveTrackedDockable(IRootDock rootDock)
    {
        if (rootDock.FocusedDockable is { } focusedDockable)
        {
            return focusedDockable;
        }

        var current = rootDock.ActiveDockable;
        if (current is not IDock)
        {
            return current;
        }

        var visited = new HashSet<IDockable>(ReferenceEqualityComparer.Instance);
        while (current is IDock dock
               && dock.ActiveDockable is { } activeDockable
               && visited.Add(current))
        {
            current = activeDockable;
        }

        return current;
    }

    /// <summary>
    /// Resolves a dockable container to the focused or deepest active dockable used for global tracking.
    /// </summary>
    /// <param name="dockable">The candidate dockable.</param>
    /// <returns>The resolved dockable, or null when no candidate is available.</returns>
    protected static IDockable? ResolveTrackedDockable(IDockable? dockable)
    {
        var current = dockable;
        if (current is not IDock)
        {
            return current;
        }

        var visited = new HashSet<IDockable>(ReferenceEqualityComparer.Instance);
        while (current is IDock dock && visited.Add(current))
        {
            var next = dock.FocusedDockable ?? dock.ActiveDockable;
            if (next is null)
            {
                break;
            }

            current = next;
        }

        return current;
    }

    /// <summary>
    /// Updates global dock tracking state.
    /// </summary>
    /// <param name="dockable">The current dockable.</param>
    /// <param name="rootDock">The current root dock.</param>
    /// <param name="window">The current window.</param>
    /// <param name="reason">Tracking change reason.</param>
    protected virtual void UpdateGlobalDockTracking(
        IDockable? dockable,
        IRootDock? rootDock,
        IDockWindow? window,
        DockTrackingChangeReason reason)
    {
        var resolvedRoot = rootDock ?? ResolveRootDock(dockable) ?? window?.Layout;
        var resolvedWindow = window ?? resolvedRoot?.Window;
        var resolvedDockable = dockable;

        if (resolvedDockable is null && reason == DockTrackingChangeReason.WindowActivated)
        {
            resolvedDockable = ResolveTrackedDockable(
                resolvedRoot?.FocusedDockable ?? resolvedRoot?.ActiveDockable);
        }

        var next = new GlobalDockTrackingState(resolvedDockable, resolvedRoot, resolvedWindow);
        if (IsGlobalDockTrackingEqual(_globalDockTrackingState, next))
        {
            return;
        }

        var previous = _globalDockTrackingState;
        _globalDockTrackingState = next;
        OnGlobalDockTrackingChanged(previous, next, reason);
    }

    /// <summary>
    /// Clears global dock tracking state when the tracked window is closed or removed.
    /// </summary>
    /// <param name="window">Window that is closing or being removed.</param>
    /// <param name="reason">Tracking change reason.</param>
    protected virtual void ClearGlobalDockTrackingForWindow(IDockWindow? window, DockTrackingChangeReason reason)
    {
        if (window is null)
        {
            return;
        }

        var trackedWindow = _globalDockTrackingState.Window;
        var trackedRoot = _globalDockTrackingState.RootDock;

        if (!ReferenceEquals(trackedWindow, window) && !ReferenceEquals(trackedRoot, window.Layout))
        {
            return;
        }

        UpdateGlobalDockTracking(null, null, null, reason);
    }

    /// <summary>
    /// Raises global dock tracking changed event.
    /// </summary>
    /// <param name="previous">The previous tracking state.</param>
    /// <param name="current">The current tracking state.</param>
    /// <param name="reason">Tracking change reason.</param>
    protected virtual void OnGlobalDockTrackingChanged(
        GlobalDockTrackingState previous,
        GlobalDockTrackingState current,
        DockTrackingChangeReason reason)
    {
        GlobalDockTrackingChanged?.Invoke(this, new GlobalDockTrackingChangedEventArgs(previous, current, reason));
    }

    /// <summary>
    /// Resolves root dock for the given dockable.
    /// </summary>
    /// <param name="dockable">The dockable.</param>
    /// <returns>The root dock or null.</returns>
    protected static IRootDock? ResolveRootDock(IDockable? dockable)
    {
        if (dockable is IRootDock rootDock)
        {
            return rootDock;
        }

        var current = dockable;
        while (current is not null)
        {
            if (current is IRootDock root)
            {
                return root;
            }

            current = current.Owner;
        }

        return null;
    }

    private static bool IsGlobalDockTrackingEqual(GlobalDockTrackingState previous, GlobalDockTrackingState current)
    {
        return ReferenceEquals(previous.Dockable, current.Dockable)
            && ReferenceEquals(previous.RootDock, current.RootDock)
            && ReferenceEquals(previous.Window, current.Window);
    }
}
