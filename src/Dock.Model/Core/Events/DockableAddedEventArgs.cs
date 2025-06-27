// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable added event args.
/// </summary>
public class DockableAddedEventArgs : EventArgs
{
    /// <summary>
    /// Gets added dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableAddedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The added dockable.</param>
    public DockableAddedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
