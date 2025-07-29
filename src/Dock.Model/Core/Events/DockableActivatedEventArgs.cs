// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable activated event arguments.
/// </summary>
public class DockableActivatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the activated dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableActivatedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The activated dockable.</param>
    public DockableActivatedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
} 