// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Defines the position of the SplitView pane.
/// </summary>
public enum SplitViewPanePlacement
{
    /// <summary>
    /// The pane is shown to the left of content.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The pane is shown to the right of content.
    /// </summary>
    Right = 1,

    /// <summary>
    /// The pane is shown above the content.
    /// </summary>
    Top = 2,

    /// <summary>
    /// The pane is shown below the content.
    /// </summary>
    Bottom = 3
}
