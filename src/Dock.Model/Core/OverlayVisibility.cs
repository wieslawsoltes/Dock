// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies the visibility state of an overlay panel.
/// </summary>
public enum OverlayVisibility
{
    /// <summary>
    /// Fully visible.
    /// </summary>
    Visible = 0,

    /// <summary>
    /// Hidden but still takes up layout space.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// Collapsed and takes no layout space.
    /// </summary>
    Collapsed = 2
}
