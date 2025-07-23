// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Dock.Settings;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Adorner that highlights docking areas for debugging purposes.
/// </summary>
internal class DebugOverlayAdorner : Control
{
    private static readonly Pen s_dockTargetPen = new(Brushes.Red);
    private static readonly Pen s_dragAreaPen = new(Brushes.Green);
    private static readonly Pen s_dropAreaPen = new(Brushes.Blue);

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
        }

        if (_pointerOver is { } hovered)
        {
            var text = hovered.DataContext?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                var ft = new FormattedText(
                    text,
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
}

