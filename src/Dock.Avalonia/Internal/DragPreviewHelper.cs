using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal static class DragPreviewHelper
{
    private static Window? _window;
    private static DragPreviewControl? _control;

    public static void Show(string title, PixelPoint position)
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
            Topmost = true
        };

        _window.Position = position;
        _window.Show();
    }

    public static void Move(PixelPoint position, string status)
    {
        if (_window is { } && _control is { })
        {
            _control.Status = status;
            _window.Position = position;
        }
    }

    public static void Hide()
    {
        if (_window is { })
        {
            _window.Close();
            _window = null;
            _control = null;
        }
    }
}
