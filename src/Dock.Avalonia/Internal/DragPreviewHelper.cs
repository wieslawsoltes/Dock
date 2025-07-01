using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private Window? _window;
    private DragPreviewControl? _control;

    private static IBitmap? Capture(Visual visual)
    {
        if (visual.VisualRoot is null)
        {
            return null;
        }

        var width = (int)visual.Bounds.Width;
        var height = (int)visual.Bounds.Height;

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var size = new PixelSize(width, height);
        var bitmap = new RenderTargetBitmap(size);
        bitmap.Render(visual);
        return bitmap;
    }

    public void Show(Visual preview, string title, PixelPoint position)
    {
        Hide();

        var bitmap = Capture(preview);

        _control = new DragPreviewControl
        {
            Title = title,
            Status = string.Empty,
            Preview = bitmap
        };

        _window = new Window
        {
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            CanResize = false,
            ShowActivated = false,
            Background = null,
            TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = _control,
            Topmost = true,
            Position = position
        };

        _window.Show();
    }

    public void Move(PixelPoint position, string status)
    {
        if (_window is null || _control is null)
        {
            return;
        }

        _control.Status = status;
        _window.Position = position;
    }

    public void Hide()
    {
        if (_window is null)
        {
            return;
        }
 
        _window.Close();
        _window = null;
        _control = null;
    }
}
