// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Represents the logical docking state of a dockable.
/// </summary>
[System.Flags]
public enum DockingWindowState
{
    /// <summary>
    /// Docking state is not set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Dockable is part of normal dock/tool layout.
    /// </summary>
    Docked = 1 << 0,

    /// <summary>
    /// Dockable is pinned in an auto-hide strip.
    /// </summary>
    Pinned = 1 << 1,

    /// <summary>
    /// Dockable is hosted inside a document dock.
    /// </summary>
    Document = 1 << 2,

    /// <summary>
    /// Dockable is hosted in a floating window.
    /// </summary>
    Floating = 1 << 3,

    /// <summary>
    /// Dockable is hidden.
    /// </summary>
    Hidden = 1 << 4
}
