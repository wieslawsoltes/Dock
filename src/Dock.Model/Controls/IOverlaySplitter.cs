// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Overlay splitter contract for resizing between panels.
/// </summary>
[RequiresDataTemplate]
public interface IOverlaySplitter : IDockable
{
    /// <summary>
    /// Gets or sets the orientation of the splitter.
    /// </summary>
    Orientation Orientation { get; set; }

    /// <summary>
    /// Gets or sets the thickness of the splitter in pixels.
    /// </summary>
    double Thickness { get; set; }

    /// <summary>
    /// Gets or sets whether the splitter can be dragged to resize.
    /// </summary>
    bool CanResize { get; set; }

    /// <summary>
    /// Gets or sets whether to show a preview while dragging.
    /// </summary>
    bool ResizePreview { get; set; }

    /// <summary>
    /// Gets or sets the panel before this splitter.
    /// </summary>
    IOverlayPanel? PanelBefore { get; set; }

    /// <summary>
    /// Gets or sets the panel after this splitter.
    /// </summary>
    IOverlayPanel? PanelAfter { get; set; }

    /// <summary>
    /// Gets or sets the minimum size of the panel before this splitter.
    /// </summary>
    double MinSizeBefore { get; set; }

    /// <summary>
    /// Gets or sets the minimum size of the panel after this splitter.
    /// </summary>
    double MinSizeAfter { get; set; }

    /// <summary>
    /// Gets or sets whether the splitter is currently being dragged.
    /// </summary>
    bool IsDragging { get; set; }
}
