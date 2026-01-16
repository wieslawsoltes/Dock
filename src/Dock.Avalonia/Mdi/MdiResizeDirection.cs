// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Avalonia.Mdi;

/// <summary>
/// Defines resize directions for MDI windows.
/// </summary>
[Flags]
public enum MdiResizeDirection
{
    /// <summary>
    /// No resize.
    /// </summary>
    None = 0,

    /// <summary>
    /// Resize from the left edge.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Resize from the top edge.
    /// </summary>
    Top = 2,

    /// <summary>
    /// Resize from the right edge.
    /// </summary>
    Right = 4,

    /// <summary>
    /// Resize from the bottom edge.
    /// </summary>
    Bottom = 8
}
