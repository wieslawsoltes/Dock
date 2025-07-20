using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using System.Globalization;

namespace Dock.Avalonia.Controls;

internal class DebugOverlayAdorner : Control
{
    private static readonly Pen s_dockTargetPen = new(Brushes.Red, 1);
    private static readonly Pen s_dragAreaPen = new(Brushes.Green, 1);
    private static readonly Pen s_dropAreaPen = new(Brushes.Blue, 1);

    private static readonly ImmutableSolidColorBrush s_dockTargetFill =
        new(Color.FromArgb(80, 255, 0, 0));
    private static readonly ImmutableSolidColorBrush s_dragAreaFill =
        new(Color.FromArgb(80, 0, 255, 0));
    private static readonly ImmutableSolidColorBrush s_dropAreaFill =
        new(Color.FromArgb(80, 0, 0, 255));
    private static readonly ImmutableSolidColorBrush s_textBrush = Brushes.White.ToImmutable();

    private Control? _pointerOver;

    public void SetPointerOver(Control? control)
    {
        if (_pointerOver != control)
        {
            _pointerOver = control;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (AdornerLayer.GetAdornedElement(this) is not Visual root)
        {
            return;
        }

        foreach (var visual in VisualExtensions.GetVisualDescendants(root))
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
                    Bounds.Width - ft.Bounds.Width - 4,
                    Bounds.Height - ft.Bounds.Height - 4);

                context.DrawText(s_textBrush, origin, ft);
            }
        }
    }
}

