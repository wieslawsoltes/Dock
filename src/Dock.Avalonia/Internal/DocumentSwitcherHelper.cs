using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

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

        Control content = root.DocumentSwitcherType switch
        {
            DocumentSwitcherType.VisualStudio => new DocumentSwitcherVisualControl
            {
                DataContext = root
            },
            _ => new DocumentSwitcherControl
            {
                DataContext = root
            }
        };

        _window = new DocumentSwitcherWindow
        {
            Content = content
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
