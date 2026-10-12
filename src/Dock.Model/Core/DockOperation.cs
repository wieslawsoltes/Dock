
// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defines the available dock operations.
/// </summary>
public enum DockOperation
{
    /// <summary>
    /// None dock.
    /// </summary>
    None,

    /// <summary>
    /// Fill dock.
    /// </summary>
    Fill,

    /// <summary>
    /// Dock to left.
    /// </summary>
    Left,

    /// <summary>
    /// Dock to bottom.
    /// </summary>
    Bottom,

    /// <summary>
    /// Dock to right.
    /// </summary>
    Right,

    /// <summary>
    /// Dock to top.
    /// </summary>
    Top,

    /// <summary>
    /// Dock to window.
    /// </summary>
    Window,

    /// <summary>Split to the left of the region selected by an <see cref="IDockOuterSplitPolicy"/>.</summary>
    OuterLeft,

    /// <summary>Split above the region selected by an <see cref="IDockOuterSplitPolicy"/>.</summary>
    OuterTop,

    /// <summary>Split to the right of the region selected by an <see cref="IDockOuterSplitPolicy"/>.</summary>
    OuterRight,

    /// <summary>Split below the region selected by an <see cref="IDockOuterSplitPolicy"/>.</summary>
    OuterBottom
}
