// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Describes current global dock/window focus context.
/// </summary>
public sealed class GlobalDockTrackingState
{
    private readonly WeakReference<IDockable>? _dockable;
    private readonly WeakReference<IRootDock>? _rootDock;
    private readonly WeakReference<IDockWindow>? _window;

    /// <summary>
    /// Gets an empty tracking state.
    /// </summary>
    public static GlobalDockTrackingState Empty { get; } = new(null, null, null);

    /// <summary>
    /// Gets the currently tracked dockable.
    /// </summary>
    public IDockable? Dockable => TryGet(_dockable);

    /// <summary>
    /// Gets the root dock that owns <see cref="Dockable"/>.
    /// </summary>
    public IRootDock? RootDock => TryGet(_rootDock);

    /// <summary>
    /// Gets the currently tracked dock window.
    /// </summary>
    public IDockWindow? Window => TryGet(_window);

    /// <summary>
    /// Gets the currently tracked host window.
    /// </summary>
    public IHostWindow? HostWindow => Window?.Host;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalDockTrackingState"/> class.
    /// </summary>
    /// <param name="dockable">The tracked dockable.</param>
    /// <param name="rootDock">The tracked root dock.</param>
    /// <param name="window">The tracked window.</param>
    public GlobalDockTrackingState(IDockable? dockable, IRootDock? rootDock, IDockWindow? window)
    {
        _dockable = CreateWeakReference(dockable);
        _rootDock = CreateWeakReference(rootDock);
        _window = CreateWeakReference(window);
    }

    private static WeakReference<T>? CreateWeakReference<T>(T? target) where T : class
    {
        return target is null ? null : new WeakReference<T>(target);
    }

    private static T? TryGet<T>(WeakReference<T>? reference) where T : class
    {
        if (reference is null)
        {
            return null;
        }

        return reference.TryGetTarget(out var target) ? target : null;
    }
}
