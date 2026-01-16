// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Overlay panel contract for panels within an <see cref="IOverlayDock"/>.
/// </summary>
[RequiresDataTemplate]
public interface IOverlayPanel : IDock
{
    /// <summary>
    /// Gets or sets the dockable content displayed in this panel.
    /// </summary>
    /// <remarks>
    /// Convenience property for the common single-child case.
    /// Implementations typically map this to the first entry of <see cref="IDock.VisibleDockables"/>.
    /// </remarks>
    IDockable? Content { get; set; }

    /// <summary>
    /// Gets or sets the X position of the panel (left edge).
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// Gets or sets the Y position of the panel (top edge).
    /// </summary>
    double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the panel.
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the panel.
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Gets or sets the Z-Index for panel stacking order.
    /// </summary>
    int ZIndex { get; set; }

    /// <summary>
    /// Gets or sets the anchor mode for the panel.
    /// </summary>
    OverlayAnchor Anchor { get; set; }

    /// <summary>
    /// Gets or sets the horizontal offset from the anchor point.
    /// </summary>
    double AnchorOffsetX { get; set; }

    /// <summary>
    /// Gets or sets the vertical offset from the anchor point.
    /// </summary>
    double AnchorOffsetY { get; set; }

    /// <summary>
    /// Gets or sets the panel opacity (0.0 to 1.0).
    /// </summary>
    double Opacity { get; set; }

    /// <summary>
    /// Gets or sets whether backdrop blur is enabled for this panel.
    /// </summary>
    bool UseBackdropBlur { get; set; }

    /// <summary>
    /// Gets or sets the backdrop blur radius.
    /// </summary>
    double BlurRadius { get; set; }

    /// <summary>
    /// Gets or sets the panel state.
    /// </summary>
    OverlayPanelState State { get; set; }

    /// <summary>
    /// Gets or sets whether the panel position is locked.
    /// </summary>
    bool IsPositionLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the panel size is locked.
    /// </summary>
    bool IsSizeLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is currently being dragged.
    /// </summary>
    bool IsDragging { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is currently focused.
    /// </summary>
    bool IsFocused { get; set; }

    /// <summary>
    /// Gets or sets whether the panel header is visible.
    /// </summary>
    bool ShowHeader { get; set; }

    /// <summary>
    /// Gets or sets whether the panel has a shadow.
    /// </summary>
    bool ShowShadow { get; set; }

    /// <summary>
    /// Gets or sets the corner radius for rounded corners.
    /// </summary>
    double CornerRadius { get; set; }

    /// <summary>
    /// Gets or sets the splitter group this panel belongs to.
    /// </summary>
    IOverlaySplitterGroup? SplitterGroup { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of content within the panel.
    /// </summary>
    OverlayHorizontalAlignment HorizontalContentAlignment { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of content within the panel.
    /// </summary>
    OverlayVerticalAlignment VerticalContentAlignment { get; set; }

    /// <summary>
    /// Gets or sets the visibility state of the panel.
    /// </summary>
    OverlayVisibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets whether to animate visibility changes.
    /// </summary>
    bool AnimateVisibility { get; set; }

    /// <summary>
    /// Gets or sets the visibility animation type.
    /// </summary>
    OverlayVisibilityAnimation VisibilityAnimation { get; set; }

    /// <summary>
    /// Gets or sets the visibility animation duration in milliseconds.
    /// </summary>
    double VisibilityAnimationDuration { get; set; }

    /// <summary>
    /// Gets or sets whether double-clicking the header floats the panel.
    /// </summary>
    bool FloatOnDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets whether other dockables can be docked into this panel.
    /// </summary>
    bool AllowDockInto { get; set; }

    /// <summary>
    /// Gets or sets the nested dock layout when the panel hosts docked content.
    /// </summary>
    IDock? NestedDock { get; set; }

    /// <summary>
    /// Gets or sets whether to show dock target indicators when dragging over this panel.
    /// </summary>
    bool ShowDockTargets { get; set; }
}
