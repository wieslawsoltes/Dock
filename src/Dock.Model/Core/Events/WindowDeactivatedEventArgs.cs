// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window deactivated event arguments.
/// </summary>
public class WindowDeactivatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the deactivated window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="WindowDeactivatedEventArgs"/> class.
    /// </summary>
    /// <param name="window">The deactivated window.</param>
    public WindowDeactivatedEventArgs(IDockWindow? window)
    {
        Window = window;
    }
} 