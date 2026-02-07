// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Global dock tracking changed event arguments.
/// </summary>
public class GlobalDockTrackingChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets previous global tracking state.
    /// </summary>
    public GlobalDockTrackingState Previous { get; }

    /// <summary>
    /// Gets current global tracking state.
    /// </summary>
    public GlobalDockTrackingState Current { get; }

    /// <summary>
    /// Gets tracking change reason.
    /// </summary>
    public DockTrackingChangeReason Reason { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="GlobalDockTrackingChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previous">The previous tracking state.</param>
    /// <param name="current">The current tracking state.</param>
    /// <param name="reason">The change reason.</param>
    public GlobalDockTrackingChangedEventArgs(
        GlobalDockTrackingState previous,
        GlobalDockTrackingState current,
        DockTrackingChangeReason reason)
    {
        Previous = previous ?? throw new ArgumentNullException(nameof(previous));
        Current = current ?? throw new ArgumentNullException(nameof(current));
        Reason = reason;
    }
}
