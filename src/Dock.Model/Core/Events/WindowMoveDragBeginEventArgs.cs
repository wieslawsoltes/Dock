// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window begin dragging event args.
/// </summary>
public record WindowMoveDragBeginEventArgs(IDockWindow? Window) : EventArgs
{
    /// <summary>
    /// Gets or sets flag indicating whether window dragging should be canceled.
    /// </summary>
    public bool Cancel { get; set; }
}
