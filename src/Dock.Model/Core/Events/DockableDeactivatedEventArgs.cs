// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable deactivated event arguments.
/// </summary>
public class DockableDeactivatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the deactivated dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableDeactivatedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The deactivated dockable.</param>
    public DockableDeactivatedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
} 