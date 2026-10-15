using Avalonia.Input;
using AvalonDock.Infrastructure;

namespace AvalonDock;

public partial class DockingManager
{
    private bool _suppressContextMenuKeyRelease;

    /// <summary>Opens the current active content's document or tool menu for Shift+F10.</summary>
    /// <remarks>The base hook follows the active content's visual parents to its public content control.
    /// It opens that control's menu on key press without changing the key event's handled state.
    /// Content-owned menus retain Avalonia's normal context-request route. Suppressing this base call
    /// also prevents a second docking-menu request on the corresponding key release.</remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (LayoutKeyboardContextMenu.IsShortcut(e) && !LayoutKeyboardContextMenu.HasContentMenu(e.Source, this))
            LayoutKeyboardContextMenu.Show(this, _contextMenus);
        base.OnKeyDown(e);
    }

    private void PrepareContextMenuKey(KeyEventArgs e)
    {
        if (e.Key == Key.F10)
            _suppressContextMenuKeyRelease = LayoutKeyboardContextMenu.IsShortcut(e)
                && !LayoutKeyboardContextMenu.HasContentMenu(e.Source, this);
    }

    private void CompleteContextMenuKey(KeyEventArgs e)
    {
        if (e.Key != Key.F10) return;
        bool suppress = _suppressContextMenuKeyRelease;
        _suppressContextMenuKeyRelease = false;
        if (suppress) e.Handled = true;
    }
}
