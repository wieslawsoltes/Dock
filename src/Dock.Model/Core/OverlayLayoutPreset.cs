// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Predefined layout arrangements for overlay panels.
/// </summary>
public enum OverlayLayoutPreset
{
    /// <summary>
    /// No automatic arrangement.
    /// </summary>
    None = 0,

    /// <summary>
    /// Stack all panels on the left edge.
    /// </summary>
    StackLeft = 1,

    /// <summary>
    /// Stack all panels on the right edge.
    /// </summary>
    StackRight = 2,

    /// <summary>
    /// Cascade panels from top-left.
    /// </summary>
    Cascade = 3,

    /// <summary>
    /// Tile panels in a grid pattern.
    /// </summary>
    TileGrid = 4,

    /// <summary>
    /// Arrange panels in corners.
    /// </summary>
    Corners = 5,

    /// <summary>
    /// Minimize all panels.
    /// </summary>
    MinimizeAll = 6,

    /// <summary>
    /// Restore all panels to normal state.
    /// </summary>
    RestoreAll = 7
}
