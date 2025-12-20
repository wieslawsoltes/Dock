// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Defines how the SplitView pane is displayed.
/// </summary>
public enum SplitViewDisplayMode
{
    /// <summary>
    /// Pane is displayed next to content and does not auto-collapse.
    /// When closed, the pane is completely hidden.
    /// </summary>
    Inline = 0,

    /// <summary>
    /// Pane is displayed next to content. When closed, the pane shows
    /// a compact view based on <see cref="Controls.ISplitViewDock.CompactPaneLength"/>.
    /// Does not auto-collapse.
    /// </summary>
    CompactInline = 1,

    /// <summary>
    /// Pane overlays the content when open.
    /// When closed, the pane is completely hidden.
    /// Auto-collapses when user interacts outside the pane.
    /// </summary>
    Overlay = 2,

    /// <summary>
    /// Pane overlays the content when open.
    /// When closed, the pane shows a compact view based on
    /// <see cref="Controls.ISplitViewDock.CompactPaneLength"/>.
    /// Auto-collapses when user interacts outside the pane.
    /// </summary>
    CompactOverlay = 3
}
