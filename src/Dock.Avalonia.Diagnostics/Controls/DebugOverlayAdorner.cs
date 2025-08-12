// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Dock.Settings;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Adorner that highlights docking areas for debugging purposes.
/// </summary>
internal class DebugOverlayAdorner : Control
{
    private static readonly Pen s_dockTargetPen = new(Brushes.Red);
    private static readonly Pen s_dragAreaPen = new(Brushes.Green);
    private static readonly Pen s_dropAreaPen = new(Brushes.Blue);
    private static readonly Pen s_invalidDropPen = new(Brushes.Red) { Thickness = 2 };

    private static readonly ImmutableSolidColorBrush s_dockTargetFill =
        new(Color.FromArgb(80, 255, 0, 0));
    private static readonly ImmutableSolidColorBrush s_dragAreaFill =
        new(Color.FromArgb(80, 0, 255, 0));
    private static readonly ImmutableSolidColorBrush s_dropAreaFill =
        new(Color.FromArgb(80, 0, 0, 255));
    private static readonly ImmutableSolidColorBrush s_textBrush = (ImmutableSolidColorBrush)Brushes.White.ToImmutable();

    private Control? _pointerOver;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugOverlayAdorner"/> class.
    /// </summary>
    public DebugOverlayAdorner()
    {
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Sets the control currently under the pointer.
    /// </summary>
    /// <param name="control">The hovered control or <c>null</c>.</param>
    public void SetPointerOver(Control? control)
    {
        if (_pointerOver != control)
        {
            _pointerOver = control;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Draws the overlay using the given drawing context.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (AdornerLayer.GetAdornedElement(this) is not { } root)
        {
            return;
        }

        var isDragging = (root as DockControl)?.IsDraggingDock == true;

        foreach (var visual in global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(root))
        {
            if (visual is not Control control)
            {
                continue;
            }

            var pos = control.TranslatePoint(new Point(), root);
            if (pos is null)
            {
                continue;
            }

            var rect = new Rect(pos.Value, control.Bounds.Size);

            if (DockProperties.GetIsDockTarget(control))
            {
                var brush = control == _pointerOver ? s_dockTargetFill : null;
                context.DrawRectangle(brush, s_dockTargetPen, rect);
            }

            if (DockProperties.GetIsDragArea(control))
            {
                var brush = control == _pointerOver ? s_dragAreaFill : null;
                context.DrawRectangle(brush, s_dragAreaPen, rect);
            }

            if (DockProperties.GetIsDropArea(control))
            {
                var brush = control == _pointerOver ? s_dropAreaFill : null;
                context.DrawRectangle(brush, s_dropAreaPen, rect);
            }

            // While dragging, show a red cross over areas that are not valid drop targets
            if (isDragging && control == _pointerOver)
            {
                var isDropArea = DockProperties.GetIsDropArea(control);
                var isDropEnabled = control.GetValue(DockProperties.IsDropEnabledProperty);
                if (!(isDropArea && isDropEnabled))
                {
                    DrawCross(context, rect, s_invalidDropPen);
                }
            }

            // Draw hatched background and label for controls that define a DockGroup
            var group = DockProperties.GetDockGroup(control);
            var isTargetArea = DockProperties.GetIsDockTarget(control) || DockProperties.GetIsDropArea(control);
            if (!string.IsNullOrEmpty(group) && isTargetArea)
            {
                var g = group!;
                var hatchColor = GetColorFromString(g, 72);
                var hatchPen = new Pen(new ImmutableSolidColorBrush(hatchColor), 1);
                DrawHatch(context, rect, hatchPen);
                DrawGroupLabel(context, rect, g, hatchColor);
            }
        }

        if (_pointerOver is { } hovered)
        {
            var text = hovered.DataContext?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                var ft = new FormattedText(
                    text!,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    12,
                    s_textBrush);

                var origin = new Point(
                    Bounds.Width - ft.Width - 4,
                    Bounds.Height - ft.Height - 4);

                context.DrawText(ft, origin);
            }
        }
    }

    private static void DrawCross(DrawingContext context, Rect rect, Pen pen)
    {
        var p1 = rect.TopLeft;
        var p2 = rect.BottomRight;
        var p3 = rect.TopRight;
        var p4 = rect.BottomLeft;
        context.DrawLine(pen, p1, p2);
        context.DrawLine(pen, p3, p4);
    }

    private static void DrawHatch(DrawingContext context, Rect rect, Pen pen)
    {
        const double spacing = 16.0; // slightly wider spacing for better readability
        using (context.PushClip(new RoundedRect(rect)))
        {
            // Primary 45° diagonal lines
            for (double x = rect.X - rect.Height; x < rect.Right; x += spacing)
            {
                var start = new Point(x, rect.Bottom);
                var end = new Point(x + rect.Height, rect.Top);
                context.DrawLine(pen, start, end);
            }
        }
    }

    private static Point ClampToRect(Point p, Rect rect)
    {
        var x = p.X;
        if (x < rect.Left) x = rect.Left; else if (x > rect.Right) x = rect.Right;
        var y = p.Y;
        if (y < rect.Top) y = rect.Top; else if (y > rect.Bottom) y = rect.Bottom;
        return new Point(x, y);
    }

    private static void DrawGroupLabel(DrawingContext context, Rect rect, string group, Color color)
    {
        var ft = new FormattedText(
            group,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Bold),
            11,
            new ImmutableSolidColorBrush(color));

        var padding = 4.0;
        var origin = new Point(rect.X + padding, rect.Y + padding);
        context.DrawText(ft, origin);
    }

    private static Pen GetGroupHatchPen(string group)
    {
        var color = GetColorFromString(group, 72);
        return new Pen(new ImmutableSolidColorBrush(color), 1);
    }

    private static Color GetColorFromString(string input, byte alpha)
    {
        unchecked
        {
            int hash = 23;
            foreach (var ch in input)
            {
                hash = hash * 31 + ch;
            }

            double hue = (hash & 0xFFFF) % 360;
            double saturation = 0.4;
            double value = 0.9;
            var (r, g, b) = HsvToRgb(hue, saturation, value);
            return new Color(alpha, r, g, b);
        }
    }

    private static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = v - c;
        double r1, g1, b1;
        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }

        byte r = (byte)Math.Round((r1 + m) * 255);
        byte g = (byte)Math.Round((g1 + m) * 255);
        byte b = (byte)Math.Round((b1 + m) * 255);
        return (r, g, b);
    }
}

