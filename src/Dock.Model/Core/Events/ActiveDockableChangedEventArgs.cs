// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Active dockable changed event args.
/// </summary>
public class ActiveDockableChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets active dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets active dockable root dock.
    /// </summary>
    public IRootDock? RootDock { get; }

    /// <summary>
    /// Gets active dockable window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Gets active dockable host window.
    /// </summary>
    public IHostWindow? HostWindow => Window?.Host;

    /// <summary>
    /// Initializes new instance of the <see cref="ActiveDockableChangedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The active dockable.</param>
    public ActiveDockableChangedEventArgs(IDockable? dockable)
        : this(dockable, null, null)
    {
    }

    /// <summary>
    /// Initializes new instance of the <see cref="ActiveDockableChangedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The active dockable.</param>
    /// <param name="rootDock">The root dock that owns the active dockable.</param>
    /// <param name="window">The window that owns the active dockable root.</param>
    public ActiveDockableChangedEventArgs(IDockable? dockable, IRootDock? rootDock, IDockWindow? window)
    {
        Dockable = dockable;
        RootDock = rootDock;
        Window = window;
    }
}
