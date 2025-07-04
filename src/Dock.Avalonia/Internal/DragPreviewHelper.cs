using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private DragPreviewWindow? _window;
    private DragPreviewControl? _control;

    private static PixelPoint GetPositionWithinWindow(Window window, PixelPoint position, PixelPoint offset)
    {
        var screen = window.Screens.ScreenFromPoint(position);
        if (screen is { })
        {
            var target = position + offset;
            if (screen.WorkingArea.Contains(target))
            {
                return target;
            }
        }
        return position;
    }

    public void Show(IDockable dockable, PixelPoint position, PixelPoint offset)
    {
        Hide();

        _control = new DragPreviewControl
        {
            Status = string.Empty
        };

        _window = new DragPreviewWindow
        {
            Content = _control,
            DataContext = dockable
        };

        _window.Position = GetPositionWithinWindow(_window, position, offset);

        _window.Show();
    }

    public void Move(PixelPoint position, PixelPoint offset, string status)
    {
        if (_window is null || _control is null)
        {
            return;
        }

        _control.Status = status;
        _window.Position = GetPositionWithinWindow(_window, position, offset);
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
