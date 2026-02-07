// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Indicates why global dock tracking state changed.
/// </summary>
public enum DockTrackingChangeReason
{
    /// <summary>
    /// Unknown reason.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The active dockable changed.
    /// </summary>
    ActiveDockableChanged = 1,

    /// <summary>
    /// The focused dockable changed.
    /// </summary>
    FocusedDockableChanged = 2,

    /// <summary>
    /// A window was activated.
    /// </summary>
    WindowActivated = 3,

    /// <summary>
    /// A window was deactivated.
    /// </summary>
    WindowDeactivated = 4,

    /// <summary>
    /// A dockable was activated.
    /// </summary>
    DockableActivated = 5,

    /// <summary>
    /// A dockable was deactivated.
    /// </summary>
    DockableDeactivated = 6,

    /// <summary>
    /// A window was closed.
    /// </summary>
    WindowClosed = 7,

    /// <summary>
    /// A window was removed.
    /// </summary>
    WindowRemoved = 8
}
