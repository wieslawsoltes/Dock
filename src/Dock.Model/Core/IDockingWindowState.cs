// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Optional docking window state mixin that allows a view model to bind to
/// layout-backed window lifecycle and activation state.
/// </summary>
public interface IDockingWindowState
{
    /// <summary>
    /// Gets or sets whether the dockable is open in the layout.
    /// </summary>
    bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets whether the dockable is the active (focused) item.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the dockable is selected in its owner dock.
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the logical docking window state.
    /// </summary>
    DockingWindowState DockingState { get; set; }
}
