// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Overlay splitter group contract for linking panels with splitters.
/// </summary>
[RequiresDataTemplate]
public interface IOverlaySplitterGroup : IDockable
{
    /// <summary>
    /// Gets or sets the panels in this splitter group.
    /// </summary>
    IList<IOverlayPanel>? Panels { get; set; }

    /// <summary>
    /// Gets or sets the splitters between panels.
    /// </summary>
    IList<IOverlaySplitter>? Splitters { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the splitter group.
    /// </summary>
    Orientation Orientation { get; set; }

    /// <summary>
    /// Gets or sets the X position of the group (left edge).
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// Gets or sets the Y position of the group (top edge).
    /// </summary>
    double Y { get; set; }

    /// <summary>
    /// Gets or sets the total width of the group.
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Gets or sets the total height of the group.
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Gets or sets the Z-Index for the entire group.
    /// </summary>
    int ZIndex { get; set; }

    /// <summary>
    /// Gets or sets the anchor mode for the group.
    /// </summary>
    OverlayAnchor Anchor { get; set; }

    /// <summary>
    /// Gets or sets whether the group position is locked.
    /// </summary>
    bool IsPositionLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the group is currently being dragged.
    /// </summary>
    bool IsDragging { get; set; }

    /// <summary>
    /// Gets or sets the edge dock mode for the group.
    /// </summary>
    OverlayEdgeDock EdgeDock { get; set; }

    /// <summary>
    /// Gets or sets whether to show a unified header for the group.
    /// </summary>
    bool ShowGroupHeader { get; set; }

    /// <summary>
    /// Gets or sets the group title when <see cref="ShowGroupHeader"/> is true.
    /// </summary>
    string? GroupTitle { get; set; }
}
