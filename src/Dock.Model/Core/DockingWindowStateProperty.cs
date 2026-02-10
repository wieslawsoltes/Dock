// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Identifies the <see cref="IDockingWindowState"/> property that changed.
/// </summary>
public enum DockingWindowStateProperty
{
    /// <summary>
    /// The <see cref="IDockingWindowState.IsOpen"/> property.
    /// </summary>
    IsOpen,

    /// <summary>
    /// The <see cref="IDockingWindowState.IsActive"/> property.
    /// </summary>
    IsActive,

    /// <summary>
    /// The <see cref="IDockingWindowState.IsSelected"/> property.
    /// </summary>
    IsSelected,

    /// <summary>
    /// The <see cref="IDockingWindowState.DockingState"/> property.
    /// </summary>
    DockingState
}
