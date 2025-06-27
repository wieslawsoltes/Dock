// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window closing event args.
/// </summary>
public class WindowClosingEventArgs : EventArgs
{
    /// <summary>
    /// Gets closing window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Gets or sets flag indicating whether window closing should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes new instance of the <see cref="WindowClosingEventArgs"/> class.
    /// </summary>
    /// <param name="window">The closing window.</param>
    public WindowClosingEventArgs(IDockWindow? window)
    {
        Window = window;
    }
}
