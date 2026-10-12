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

    private DragPreviewSnapshot(RenderTargetBitmap bitmap)
    {
        _bitmap = bitmap;
        Source = bitmap;
        Stretch = Stretch.Uniform;
        ClipToBounds = true;
    }

    internal static DragPreviewSnapshot? Capture(Visual visual)
    {
        Size size = visual.Bounds.Size;
        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) || size.Width <= 0 || size.Height <= 0)
            return null;
        double scaling = TopLevel.GetTopLevel(visual)?.RenderScaling ?? 1;
        RenderTargetBitmap bitmap = new(PixelSize.FromSize(size, scaling), new Vector(96 * scaling, 96 * scaling));
        try
        {
            bitmap.Render(visual);
            return new DragPreviewSnapshot(bitmap);
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
