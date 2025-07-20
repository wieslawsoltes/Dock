using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using System;
using System.Threading.Tasks;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private DragPreviewWindow? _window;
    private DragPreviewControl? _control;
    private Control? _source;

    private void SourceLayoutUpdated(object? sender, EventArgs e)
    {
        if (_source is { } src && _control is { })
        {
            _control.PreviewWidth = src.Bounds.Width;
            _control.PreviewHeight = src.Bounds.Height;
        }
    }

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

    public void Show(IDockable dockable, Control source, PixelPoint position, PixelPoint offset)
    {
        Hide();

        _source = source;
        _control = new DragPreviewControl
        {
            Status = string.Empty,
            PreviewVisual = source,
            PreviewWidth = source.Bounds.Width,
            PreviewHeight = source.Bounds.Height
        };

        source.LayoutUpdated += SourceLayoutUpdated;

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

        if (_source is { })
        {
            _source.LayoutUpdated -= SourceLayoutUpdated;
            _source = null;
        }

        _control = null;
    }
}
