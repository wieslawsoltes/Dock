using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Dock.Avalonia.Internal;

// Capture once before the source layout hides, recycles or selects another tab.
// The preview owns only pixels and cannot retain or reparent the source editor.
internal sealed class DragPreviewSnapshot : Image, IDisposable
{
    private RenderTargetBitmap? _bitmap;

    private DragPreviewSnapshot(RenderTargetBitmap bitmap, Size size)
    {
        _bitmap = bitmap;
        Source = bitmap;
        Width = size.Width;
        Height = size.Height;
        Stretch = Stretch.Uniform;
        ClipToBounds = true;
    }

    internal static DragPreviewSnapshot? Capture(Visual visual)
    {
        Size size = visual.Bounds.Size;
        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) || size.Width <= 0 || size.Height <= 0)
            return null;
        double scaling = TopLevel.GetTopLevel(visual)?.RenderScaling ?? 1;
        // Store at 96 DPI so Image's source rectangle addresses every pixel on
        // both immediate and composition renderers. Preserve the logical size
        // on this control and rasterize the visual at the host's pixel density.
        RenderTargetBitmap bitmap = new(PixelSize.FromSize(size, scaling));
        try
        {
            using (DrawingContext context = bitmap.CreateDrawingContext())
                context.DrawRectangle(new VisualBrush(visual) { Stretch = Stretch.Fill }, null, new Rect(bitmap.Size));
            return new DragPreviewSnapshot(bitmap, size);
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        Source = null;
        _bitmap?.Dispose();
        _bitmap = null;
    }
}
