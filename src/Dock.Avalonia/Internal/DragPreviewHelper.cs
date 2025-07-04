using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private DragPreviewWindow? _window;
    private DragPreviewControl? _control;

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
            Position = position + offset,
            DataContext = dockable
        };

        _window.Show();
    }

    public void Move(PixelPoint position, PixelPoint offset, string status)
    {
        if (_window is null || _control is null)
        {
            return;
        }

        _control.Status = status;
        _window.Position = position + offset;
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
