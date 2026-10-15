using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AvalonDock.Infrastructure;

// ContextMenu retains its previous focus in Avalonia 12. Keep that reference
// weak here, including when a menu is held in application resources.
internal sealed class LayoutContextMenuFocus
{
    private WeakReference<IInputElement>? _previous;

    public void Capture(Control target)
    {
        if (TopLevel.GetTopLevel(target)?.FocusManager is not { } manager) return;
        if (manager.GetFocusedElement() is { } previous) _previous = new(previous);
        manager.Focus(null);
    }

    public void Restore()
    {
        WeakReference<IInputElement>? reference = _previous;
        _previous = null;
        if (reference?.TryGetTarget(out IInputElement? previous) != true || previous is not Visual visual ||
            TopLevel.GetTopLevel(visual)?.FocusManager is not { } manager) return;
        // A command may have focused another control or opened a dialog.
        if (manager.GetFocusedElement() is not Visual current || TopLevel.GetTopLevel(current) is null)
            manager.Focus(previous);
    }
}
