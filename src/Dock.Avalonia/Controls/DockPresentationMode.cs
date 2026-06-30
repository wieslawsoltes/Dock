// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Avalonia.Controls;

/// <summary>
/// Defines how proportional docks are presented in the Avalonia visual layer.
/// </summary>
public enum DockPresentationMode
{
    /// <summary>
    /// Presents the dock model with the existing nested proportional dock controls.
    /// </summary>
    Nested = 0,

    /// <summary>
    /// Presents proportional dock descendants through a flattened panel surface.
    /// </summary>
    Flat = 1
}
