// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Represents the presentation state of a floating dock window.
/// </summary>
public enum DockWindowState
{
    /// <summary>
    /// Window is shown at its normal size and position.
    /// </summary>
    Normal,

    /// <summary>
    /// Window is minimized.
    /// </summary>
    Minimized,

    /// <summary>
    /// Window is maximized.
    /// </summary>
    Maximized,

    /// <summary>
    /// Window is shown in full-screen mode.
    /// </summary>
    FullScreen
}
