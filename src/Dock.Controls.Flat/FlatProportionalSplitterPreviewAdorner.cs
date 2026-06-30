// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Dock.Controls.Flat;

internal sealed class FlatProportionalSplitterPreviewAdorner : Control
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<FlatProportionalSplitterPreviewAdorner, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<FlatProportionalSplitterPreviewAdorner, double>(nameof(Thickness), 4.0);

    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<FlatProportionalSplitterPreviewAdorner, double>(nameof(Offset));

    public static readonly StyledProperty<IBrush?> PreviewBrushProperty =
        AvaloniaProperty.Register<FlatProportionalSplitterPreviewAdorner, IBrush?>(
            nameof(PreviewBrush),
            new SolidColorBrush(Color.FromArgb(96, 0, 120, 212)));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public IBrush? PreviewBrush
    {
        get => GetValue(PreviewBrushProperty);
        set => SetValue(PreviewBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (PreviewBrush is not { } brush)
        {
            return;
        }

        var thickness = Math.Max(1.0, Thickness);
        var rect = Orientation == Avalonia.Layout.Orientation.Vertical
            ? new Rect(0, Offset, Bounds.Width, thickness)
            : new Rect(Offset, 0, thickness, Bounds.Height);

        context.FillRectangle(brush, rect);
    }
}
