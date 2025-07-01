using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Dock.Avalonia.Internal;

internal static class DragPreviewHelper
{
    private static Window? _window;

    public static void Show(string title, PixelPoint position)
    {
        Hide();

        var border = new Border
        {
            Background = Brushes.Gray,
            Padding = new Thickness(4,2),
            Child = new TextBlock { Text = title, Foreground = Brushes.White }
        };

        _window = new Window
        {
            Width = 120,
            Height = 30,
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            CanResize = false,
            ShowActivated = false,
            Background = null,
            Content = border,
            Topmost = true
        };

        _window.Position = position;
        _window.Show();
    }

    public static void Move(PixelPoint position)
    {
        if (_window is { })
        {
            _window.Position = position;
        }
    }

    public static void Hide()
    {
        if (_window is { })
        {
            _window.Close();
            _window = null;
        }
    }
}
