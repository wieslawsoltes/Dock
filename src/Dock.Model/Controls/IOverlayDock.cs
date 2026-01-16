// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Overlay dock contract.
/// Provides a container with a full-size background content and floating overlay panels.
/// </summary>
[RequiresDataTemplate]
public interface IOverlayDock : IDock, IGlobalTarget
{
    /// <summary>
    /// Gets or sets the background dockable that fills the entire area.
    /// </summary>
    IDockable? BackgroundDockable { get; set; }

    /// <summary>
    /// Gets or sets the collection of overlay panels.
    /// </summary>
    IList<IOverlayPanel>? OverlayPanels { get; set; }

    /// <summary>
    /// Gets or sets the collection of splitter groups.
    /// </summary>
    IList<IOverlaySplitterGroup>? SplitterGroups { get; set; }

    /// <summary>
    /// Gets or sets whether overlay panels can be dragged to reposition.
    /// </summary>
    bool AllowPanelDragging { get; set; }

    /// <summary>
    /// Gets or sets whether overlay panels can be resized.
    /// </summary>
    bool AllowPanelResizing { get; set; }

    /// <summary>
    /// Gets or sets whether panels should snap to edges and corners.
    /// </summary>
    bool EnableSnapToEdge { get; set; }

    /// <summary>
    /// Gets or sets the snap threshold distance in pixels.
    /// </summary>
    double SnapThreshold { get; set; }

    /// <summary>
    /// Gets or sets whether panels should snap to other panels.
    /// </summary>
    bool EnableSnapToPanel { get; set; }

    /// <summary>
    /// Gets or sets the default opacity for new panels.
    /// </summary>
    double DefaultPanelOpacity { get; set; }

    /// <summary>
    /// Gets or sets whether backdrop blur is enabled for panels.
    /// </summary>
    bool EnableBackdropBlur { get; set; }

    /// <summary>
    /// Gets or sets whether to show grid lines for panel alignment.
    /// </summary>
    bool ShowAlignmentGrid { get; set; }

    /// <summary>
    /// Gets or sets the grid size for alignment when <see cref="ShowAlignmentGrid"/> is true.
    /// </summary>
    double AlignmentGridSize { get; set; }
}
