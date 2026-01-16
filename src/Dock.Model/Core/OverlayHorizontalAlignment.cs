// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies horizontal alignment of content within an overlay panel.
/// </summary>
public enum OverlayHorizontalAlignment
{
    /// <summary>
    /// Aligned to the left edge.
    /// </summary>
    Left = 0,

    /// <summary>
    /// Centered horizontally.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Aligned to the right edge.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Stretches to fill the available width.
    /// </summary>
    Stretch = 3
}
