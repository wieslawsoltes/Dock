// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace Dock.Controls.ProportionalStackPanel;

internal class SplitterPreviewAdorner : Control
{
    public Orientation Orientation { get; set; }
    public double Thickness { get; set; }
    public double Offset { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var host = AdornerLayer.GetAdornedElement(this);
        if (host is null)
        {
            return;
        }

        var brush = host.FindResource("DockApplicationAccentBrushIndicator") as IBrush ?? Brushes.Gray;
        var rect = Orientation == Orientation.Vertical
            ? new Rect(0, Offset, host.Bounds.Width, Thickness)
            : new Rect(Offset, 0, Thickness, host.Bounds.Height);
        context.DrawRectangle(brush, null, rect);
    }
}
