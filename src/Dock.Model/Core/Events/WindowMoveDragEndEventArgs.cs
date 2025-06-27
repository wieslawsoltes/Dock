// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window dragging ended event args.
/// </summary>
public class WindowMoveDragEndEventArgs : EventArgs
{
    /// <summary>
    /// Gets dragged window.
    /// </summary>
    public IDockWindow? Window { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="WindowMoveDragEndEventArgs"/> class.
    /// </summary>
    /// <param name="window">The dragged window.</param>
    public WindowMoveDragEndEventArgs(IDockWindow? window)
    {
        Window = window;
    }
}
