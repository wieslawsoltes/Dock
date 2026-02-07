// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Describes current global dock/window focus context.
/// </summary>
public sealed class GlobalDockTrackingState
{
    /// <summary>
    /// Gets an empty tracking state.
    /// </summary>
    public static GlobalDockTrackingState Empty { get; } = new(null, null, null);

    /// <summary>
    /// Gets the currently tracked dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets the root dock that owns <see cref="Dockable"/>.
    /// </summary>
    public IRootDock? RootDock { get; }

    /// <summary>
    /// Gets the currently tracked dock window.
    /// </summary>
    public IDockWindow? Window { get; }

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
        Dockable = dockable;
        RootDock = rootDock;
        Window = window;
    }
}
