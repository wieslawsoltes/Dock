// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core;

/// <summary>
/// Flags describing allowed docking operations.
/// </summary>
[Flags]
public enum DockOperationMask
{
    /// <summary>
    /// No docking operations are allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Allow docking as a fill operation (tab/center).
    /// </summary>
    Fill = 1 << 0,

    /// <summary>
    /// Allow docking to the left.
    /// </summary>
    Left = 1 << 1,

    /// <summary>
    /// Allow docking to the right.
    /// </summary>
    Right = 1 << 2,

    /// <summary>
    /// Allow docking to the top.
    /// </summary>
    Top = 1 << 3,

    /// <summary>
    /// Allow docking to the bottom.
    /// </summary>
    Bottom = 1 << 4,

    /// <summary>
    /// Allow docking into a new window.
    /// </summary>
    Window = 1 << 5,

    /// <summary>
    /// Allow all docking operations.
    /// </summary>
    All = Fill | Left | Right | Top | Bottom | Window
}
