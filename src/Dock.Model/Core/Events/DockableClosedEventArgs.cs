// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable closed event args.
/// </summary>
public class DockableClosedEventArgs : EventArgs
{
    /// <summary>
    /// Gets closed dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableRemovedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The closed dockable.</param>
    public DockableClosedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
