﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Avalonia.Internal;

/// <summary>
/// Pointer event type.
/// </summary>
public enum EventType
{
    /// <summary>
    /// Pointer pressed.
    /// </summary>
    Pressed,
    /// <summary>
    /// Pointer released.
    /// </summary>
    Released,
    /// <summary>
    /// Pointer moved.
    /// </summary>
    Moved,
    /// <summary>
    /// Pointer enter.
    /// </summary>
    Enter,
    /// <summary>
    /// Pointer leave.
    /// </summary>
    Leave,
    /// <summary>
    /// Lost capture.
    /// </summary>
    CaptureLost,
    /// <summary>
    /// Wheel changed.
    /// </summary>
    WheelChanged
}
