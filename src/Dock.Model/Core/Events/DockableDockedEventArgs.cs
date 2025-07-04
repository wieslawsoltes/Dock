// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable docked event args.
/// </summary>
public class DockableDockedEventArgs : EventArgs
{
    /// <summary>
    /// Gets docked dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets dock operation.
    /// </summary>
    public DockOperation Operation { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableDockedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The docked dockable.</param>
    /// <param name="operation">Dock operation that was performed.</param>
    public DockableDockedEventArgs(IDockable? dockable, DockOperation operation)
    {
        Dockable = dockable;
        Operation = operation;
    }
}
