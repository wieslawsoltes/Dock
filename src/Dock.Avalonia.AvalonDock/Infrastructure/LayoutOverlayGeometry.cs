using System;
using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

internal static class LayoutOverlayGeometry
{
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
