// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window activated event arguments.
/// </summary>
public class WindowActivatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the activated window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="WindowActivatedEventArgs"/> class.
    /// </summary>
    /// <param name="window">The activated window.</param>
    public WindowActivatedEventArgs(IDockWindow? window)
    {
        Window = window;
    }
} 