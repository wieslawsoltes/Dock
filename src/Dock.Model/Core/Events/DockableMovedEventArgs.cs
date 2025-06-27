// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable moved event args.
/// </summary>
public class DockableMovedEventArgs : EventArgs
{
    /// <summary>
    /// Gets moved dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableMovedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The moved dockable.</param>
    public DockableMovedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
