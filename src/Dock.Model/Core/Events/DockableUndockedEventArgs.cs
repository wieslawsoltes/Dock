// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable undocked event args.
/// </summary>
public class DockableUndockedEventArgs : EventArgs
{
    /// <summary>
    /// Gets undocked dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets dock operation.
    /// </summary>
    public DockOperation Operation { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableUndockedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The undocked dockable.</param>
    /// <param name="operation">Dock operation that was performed.</param>
    public DockableUndockedEventArgs(IDockable? dockable, DockOperation operation)
    {
        Dockable = dockable;
        Operation = operation;
    }
}
