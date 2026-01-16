// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies vertical alignment of content within an overlay panel.
/// </summary>
public enum OverlayVerticalAlignment
{
    /// <summary>
    /// Aligned to the top edge.
    /// </summary>
    Top = 0,

    /// <summary>
    /// Centered vertically.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Aligned to the bottom edge.
    /// </summary>
    Bottom = 2,

    /// <summary>
    /// Stretches to fill the available height.
    /// </summary>
    Stretch = 3
}
