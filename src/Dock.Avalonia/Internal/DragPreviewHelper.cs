using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private static readonly object s_sync = new();
    private static DragPreviewWindow? s_window;
    private static DragPreviewControl? s_control;

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
        lock (s_sync)
        {
            if (s_window is null || s_control is null)
            {
                s_control = new DragPreviewControl
                {
                    Status = string.Empty
                };

                s_window = new DragPreviewWindow
                {
                    Content = s_control
                };
            }

            s_window.DataContext = dockable;
            s_control.Status = string.Empty;
            s_window.Position = GetPositionWithinWindow(s_window, position, offset);

            if (!s_window.IsVisible)
            {
                s_window.Show();
            }
        }
    }

    public void Move(PixelPoint position, PixelPoint offset, string status)
    {
        lock (s_sync)
        {
            if (s_window is null || s_control is null)
            {
                return;
            }

            s_control.Status = status;
            s_window.Position = GetPositionWithinWindow(s_window, position, offset);
        }
    }

    public void Hide()
    {
        lock (s_sync)
        {
            if (s_window is null)
            {
                return;
            }

            s_window.Close();
            s_window = null;
            s_control = null;
        }
    }
}
