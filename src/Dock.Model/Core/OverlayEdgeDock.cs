// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies the edge docking mode for overlay splitter groups.
/// </summary>
public enum OverlayEdgeDock
{
    /// <summary>
    /// Not docked to any edge.
    /// </summary>
    None = 0,

    /// <summary>
    /// Docked to the left edge, spanning full height.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Docked to the right edge, spanning full height.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Docked to the top edge, spanning full width.
    /// </summary>
    Top = 3,

    /// <summary>
    /// Docked to the bottom edge, spanning full width.
    /// </summary>
    Bottom = 4,

    /// <summary>
    /// Fills the entire overlay area.
    /// </summary>
    Fill = 5
}
