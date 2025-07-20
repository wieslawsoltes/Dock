// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable closing event args.
/// </summary>
public class DockableClosingEventArgs : EventArgs
{
    /// <summary>
    /// Gets closing dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets or sets flag indicating whether dockable closing should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableClosingEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The closing dockable.</param>
    public DockableClosingEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
