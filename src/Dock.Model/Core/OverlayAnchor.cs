// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies the anchor position for an overlay panel or group.
/// </summary>
public enum OverlayAnchor
{
    /// <summary>
    /// Uses absolute X/Y coordinates.
    /// </summary>
    None = 0,

    /// <summary>
    /// Anchored to the top-left corner.
    /// </summary>
    TopLeft = 1,

    /// <summary>
    /// Anchored to the top-center.
    /// </summary>
    Top = 2,

    /// <summary>
    /// Anchored to the top-right corner.
    /// </summary>
    TopRight = 3,

    /// <summary>
    /// Anchored to the left edge, vertically centered.
    /// </summary>
    Left = 4,

    /// <summary>
    /// Anchored to the center of the container.
    /// </summary>
    Center = 5,

    /// <summary>
    /// Anchored to the right edge, vertically centered.
    /// </summary>
    Right = 6,

    /// <summary>
    /// Anchored to the bottom-left corner.
    /// </summary>
    BottomLeft = 7,

    /// <summary>
    /// Anchored to the bottom-center.
    /// </summary>
    Bottom = 8,

    /// <summary>
    /// Anchored to the bottom-right corner.
    /// </summary>
    BottomRight = 9
}
