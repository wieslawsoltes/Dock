
// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defines the available dock operations.
/// </summary>
public enum DockOperation
{
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

    /// <summary>
    /// Dock to root left.
    /// </summary>
    RootLeft,

    /// <summary>
    /// Dock to root bottom.
    /// </summary>
    RootBottom,

    /// <summary>
    /// Dock to root right.
    /// </summary>
    RootRight,

    /// <summary>
    /// Dock to root top.
    /// </summary>
    RootTop
}
