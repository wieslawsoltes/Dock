// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Synchronizes state requests from <see cref="IDockingWindowState"/> instances
/// back to the docking layout.
/// </summary>
public interface IDockingWindowStateSync
{
    /// <summary>
    /// Handles a property change request from a docking window state mixin.
    /// </summary>
    /// <param name="dockable">The dockable that changed.</param>
    /// <param name="property">The changed property.</param>
    void OnDockingWindowStatePropertyChanged(IDockable dockable, DockingWindowStateProperty property);
}
