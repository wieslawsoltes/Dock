// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies the state of an overlay panel.
/// </summary>
public enum OverlayPanelState
{
    /// <summary>
    /// Normal floating state.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Minimized to just the header/title bar.
    /// </summary>
    Minimized = 1,

    /// <summary>
    /// Maximized to fill the overlay dock area.
    /// </summary>
    Maximized = 2,

    /// <summary>
    /// Collapsed to an icon/tab on the edge.
    /// </summary>
    AutoHide = 3,

    /// <summary>
    /// Docked to an edge but overlays content.
    /// </summary>
    Docked = 4
}
