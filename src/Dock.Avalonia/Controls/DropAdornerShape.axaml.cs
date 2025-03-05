using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaDock = Avalonia.Controls.Dock;

namespace Dock.Avalonia.Controls;

public class DropAdornerShape : Control
{
    public static readonly StyledProperty<IBrush?> ForegroundProperty = 
        AvaloniaProperty.Register<DropAdornerShape, IBrush?>(nameof(Foreground));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty = 
        AvaloniaProperty.Register<DropAdornerShape, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<IBrush?> BackgroundProperty = 
        AvaloniaProperty.Register<DropAdornerShape, IBrush?>(nameof(Background));

    public static readonly StyledProperty<AvaloniaDock?> DockPositionProperty = 
        AvaloniaProperty.Register<DropAdornerShape, AvaloniaDock?>(nameof(DockPosition));

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public AvaloniaDock? DockPosition
    {
        get => GetValue(DockPositionProperty);
        set => SetValue(DockPositionProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(40, 40);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(this.Bounds.Size));
        context.DrawRectangle(Background, new Pen(BorderBrush, 1), new Rect(new Size(40, 40)), 3, 3);
        context.DrawGeometry(Foreground, null, CreateGeometry(DockPosition, 40));
    }

    private Geometry CreateGeometry(AvaloniaDock? dock, double size)
    {
        var streamGeometry = new StreamGeometry();
        using (var context = streamGeometry.Open())
        {
            // Draw a triangle. 
            if (dock is not null)
            {
                Point[] dots = dock switch
                {
                    AvaloniaDock.Right => [new Point(10, 16), new Point(14, 20), new Point(10, 24)],
                    AvaloniaDock.Left => [new Point(30, 16), new Point(26, 20), new Point(30, 24)],
                    AvaloniaDock.Top => [new Point(16, 30), new Point(20, 26), new Point(24, 30)],
                    AvaloniaDock.Bottom => [new Point(16, 10), new Point(20, 14), new Point(24, 10)],
                    _ => throw new ArgumentOutOfRangeException(nameof(dock), dock, null)
                };
                context.BeginFigure(dots[0], true);
                context.LineTo(dots[1]);
                context.LineTo(dots[2]);
                context.EndFigure(true);
            }

            Point[] outerDots = dock switch
            {
                AvaloniaDock.Right =>
                [
                    new Point(20, 6), new Point(34, 6), new Point(34, 31), new Point(31, 34), new Point(23, 34),
                    new Point(20, 31), new Point(20, 6)
                ],
                AvaloniaDock.Left =>
                [
                    new Point(6, 6), new Point(20, 6), new Point(20, 31), new Point(17, 34), new Point(9, 34),
                    new Point(6, 31), new Point(6, 6)
                ],
                AvaloniaDock.Top =>
                [
                    new Point(6, 6), new Point(34, 6), new Point(34, 17), new Point(31, 20), new Point(9, 20),
                    new Point(6, 17), new Point(6, 6)
                ],
                AvaloniaDock.Bottom =>
                [
                    new Point(6, 20), new Point(34, 20), new Point(34, 31), new Point(31, 34), new Point(9, 34),
                    new Point(6, 31), new Point(6, 6)
                ],
                _ =>
                [
                    new Point(6, 6), new Point(34, 6), new Point(34, 31), new Point(31, 34), new Point(9, 34),
                    new Point(6, 31), new Point(6, 6)
                ]
            };

            context.BeginFigure(outerDots[0], true);
            context.LineTo(outerDots[1]);
            context.LineTo(outerDots[2]);
            context.ArcTo(outerDots[3], new Size(3, 3), 45, false, SweepDirection.Clockwise);
            context.LineTo(outerDots[4]);
            context.ArcTo(outerDots[5], new Size(3, 3), 45, false, SweepDirection.Clockwise);
            context.LineTo(outerDots[6]);
            context.EndFigure(true);
        }

        var innerStreamGeometry = new StreamGeometry();
        using (var innerContext = innerStreamGeometry.Open())
        {
            Point[] outerDots = dock switch
            {
                AvaloniaDock.Right =>
                [
                    new Point(21, 10), new Point(33, 10), new Point(33, 31), new Point(31, 33), new Point(23, 33),
                    new Point(21, 31), new Point(21, 10)
                ],
                AvaloniaDock.Left =>
                [
                    new Point(7, 10), new Point(19, 10), new Point(19, 31), new Point(17, 33), new Point(9, 33),
                    new Point(7, 31), new Point(7, 10)
                ],
                AvaloniaDock.Top =>
                [
                    new Point(7, 10), new Point(33, 10), new Point(33, 17), new Point(31, 19), new Point(9, 19),
                    new Point(7, 17), new Point(7, 10)
                ],
                AvaloniaDock.Bottom =>
                [
                    new Point(7, 24), new Point(33, 24), new Point(33, 31), new Point(31, 33), new Point(9, 33),
                    new Point(7, 31), new Point(7, 24)
                ],
                _ =>
                [
                    new Point(7, 10), new Point(33, 10), new Point(33, 31), new Point(31, 33), new Point(9, 33),
                    new Point(7, 31), new Point(7, 10)
                ]
            };
            innerContext.BeginFigure(outerDots[0], true);
            innerContext.LineTo(outerDots[1]);
            innerContext.LineTo(outerDots[2]);
            innerContext.ArcTo(outerDots[3], new Size(2, 2), 45, false, SweepDirection.Clockwise);
            innerContext.LineTo(outerDots[4]);
            innerContext.ArcTo(outerDots[5], new Size(2, 2), 45, false, SweepDirection.Clockwise);
            innerContext.LineTo(outerDots[6]);
            innerContext.EndFigure(true);
        }

        return new CombinedGeometry(GeometryCombineMode.Exclude, streamGeometry, innerStreamGeometry);
    }
}
