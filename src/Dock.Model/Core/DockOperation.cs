﻿
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
    /// Dock to global left.
    /// </summary>
    GlobalLeft,

    /// <summary>
    /// Dock to global bottom.
    /// </summary>
    GlobalBottom,

    /// <summary>
    /// Dock to global right.
    /// </summary>
    GlobalRight,

    /// <summary>
    /// Dock to global top.
    /// </summary>
    GlobalTop,

    /// <summary>
    /// Dock to window.
    /// </summary>
    Window
}
