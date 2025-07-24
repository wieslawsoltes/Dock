// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Adorner that highlights a selected control with a rectangle.
/// </summary>
internal class SelectionOverlayAdorner : Control
{
    private static readonly Pen s_pen = new(Brushes.Yellow, 2);

    /// <summary>
    /// Gets or sets the rectangle to highlight.
    /// </summary>
    public Rect? HighlightRect { get; set; }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (HighlightRect is { } rect)
        {
            context.DrawRectangle(null, s_pen, rect);
        }
    }
}
