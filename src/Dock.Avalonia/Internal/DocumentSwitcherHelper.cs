using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace Dock.Avalonia.Internal;

internal class DocumentSwitcherHelper
{
    private DocumentSwitcherWindow? _window;

    public void Show(IRootDock root)
    {
        if (_window != null)
        {
            return;
        }

        var control = new DocumentSwitcherControl
        {
            DataContext = root
        };

        _window = new DocumentSwitcherWindow
        {
            Content = control
        };

        _window.Show();
    }

    public void Hide()
    {
        if (_window == null)
            return;

        _window.Close();
        _window = null;
    }
}
