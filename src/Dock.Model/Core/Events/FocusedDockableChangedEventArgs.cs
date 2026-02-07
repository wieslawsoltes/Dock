// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Focused dockable changed event args.
/// </summary>
public class FocusedDockableChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets focused dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets focused dockable root dock.
    /// </summary>
    public IRootDock? RootDock { get; }

    /// <summary>
    /// Gets focused dockable window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Gets focused dockable host window.
    /// </summary>
    public IHostWindow? HostWindow => Window?.Host;

    /// <summary>
    /// Initializes new instance of the <see cref="FocusedDockableChangedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The focused dockable.</param>
    public FocusedDockableChangedEventArgs(IDockable? dockable)
        : this(dockable, null, null)
    {
    }

    /// <summary>
    /// Initializes new instance of the <see cref="FocusedDockableChangedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The focused dockable.</param>
    /// <param name="rootDock">The root dock that owns the focused dockable.</param>
    /// <param name="window">The window that owns the focused dockable root.</param>
    public FocusedDockableChangedEventArgs(IDockable? dockable, IRootDock? rootDock, IDockWindow? window)
    {
        Dockable = dockable;
        RootDock = rootDock;
        Window = window;
    }
}
