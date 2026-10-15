using System;
using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

internal static class LayoutOverlayGeometry
{
    internal static Rect GetBounds(Control source, Rect bounds, Control destination)
    {
        TopLevel? sourceRoot = TopLevel.GetTopLevel(source);
        TopLevel? destinationRoot = TopLevel.GetTopLevel(destination);
        if (sourceRoot is null || destinationRoot is null) return default;
        bool sameRoot = ReferenceEquals(sourceRoot, destinationRoot);
        PixelPoint sourceOrigin = sameRoot ? default : sourceRoot.PointToScreen(default);
        PixelPoint destinationOrigin = sameRoot ? default : destinationRoot.PointToScreen(default);
        Point? Map(Point point)
        {
            if (sameRoot) return source.TranslatePoint(point, destination);
            if (source.TranslatePoint(point, sourceRoot) is not { } local) return null;
            // Only window origins are integral screen pixels. Keep the local
            // coordinates fractional across DPI scales and visual transforms.
            Point translated = new(
                (sourceOrigin.X - destinationOrigin.X + local.X * sourceRoot.RenderScaling) / destinationRoot.RenderScaling,
                (sourceOrigin.Y - destinationOrigin.Y + local.Y * sourceRoot.RenderScaling) / destinationRoot.RenderScaling);
            return destinationRoot.TranslatePoint(translated, destination);
        }
        if (Map(bounds.TopLeft) is not { } a || Map(bounds.TopRight) is not { } b ||
            Map(bounds.BottomLeft) is not { } c || Map(bounds.BottomRight) is not { } d) return default;
        double left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
        double top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
        return new Rect(left, top, Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X)) - left,
            Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)) - top);
    }

    internal static Rect GetScreenBounds(Control control)
    {
        if (TopLevel.GetTopLevel(control) is null) return default;
        PixelPoint a = control.PointToScreen(default);
        PixelPoint b = control.PointToScreen(new Point(control.Bounds.Width, 0));
        PixelPoint c = control.PointToScreen(new Point(0, control.Bounds.Height));
        PixelPoint d = control.PointToScreen(new Point(control.Bounds.Width, control.Bounds.Height));
        int left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
        int top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
        int right = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
        int bottom = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));
        return new Rect(left, top, right - left, bottom - top);
    }
}
