// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Contract;

/// <summary>
/// Provides a method for calculating drag offsets when displaying preview windows.
/// </summary>
public interface IDragOffsetCalculator
{
    /// <summary>
    /// Calculates the drag offset for the preview window.
    /// </summary>
    /// <param name="dragControl">The control being dragged.</param>
    /// <param name="dockControl">The originating <see cref="DockControl"/>.</param>
    /// <param name="pointerPosition">Pointer position relative to <paramref name="dockControl"/>.</param>
    /// <returns>The calculated offset in screen coordinates.</returns>
    PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition);
}
