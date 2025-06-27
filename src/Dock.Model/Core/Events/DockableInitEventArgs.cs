// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable init event args.
/// </summary>
public class DockableInitEventArgs : EventArgs
{
    /// <summary>
    /// Gets init dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets or sets dockable context.
    /// </summary>
    public object? Context { get; set; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableInitEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The init dockable.</param>
    public DockableInitEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
