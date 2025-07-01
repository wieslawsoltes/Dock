using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private Window? _window;
    private DragPreviewControl? _control;

    public void Show(string title, PixelPoint position)
    {
        Hide();

        _control = new DragPreviewControl
        {
            Title = title,
            Status = string.Empty
        };

        _window = new Window
        {
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            CanResize = false,
            ShowActivated = false,
            Background = null,
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
