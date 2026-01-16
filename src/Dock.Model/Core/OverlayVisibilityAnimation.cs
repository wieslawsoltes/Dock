// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Specifies the animation type for visibility transitions.
/// </summary>
public enum OverlayVisibilityAnimation
{
    /// <summary>
    /// No animation.
    /// </summary>
    None = 0,

    /// <summary>
    /// Fade in/out.
    /// </summary>
    Fade = 1,

    /// <summary>
    /// Slide from/to the left edge.
    /// </summary>
    SlideLeft = 2,

    /// <summary>
    /// Slide from/to the right edge.
    /// </summary>
    SlideRight = 3,

    /// <summary>
    /// Slide from/to the top edge.
    /// </summary>
    SlideTop = 4,

    /// <summary>
    /// Slide from/to the bottom edge.
    /// </summary>
    SlideBottom = 5,

    /// <summary>
    /// Scale up/down.
    /// </summary>
    Scale = 6,

    /// <summary>
    /// Combined fade and slide.
    /// </summary>
    FadeSlide = 7
}
