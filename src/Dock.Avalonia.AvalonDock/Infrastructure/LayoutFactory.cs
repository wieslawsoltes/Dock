using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

// Keep native window vetoes first, then let the compatibility layer close or
// hide content before the native host can detach the floating layout.
internal sealed class LayoutFactory(Func<IDockWindow, bool> closeWindow) : Factory
{
    private IDockWindow? _evaluatingClose;

    public override bool OnWindowClosing(IDockWindow? window)
    {
        // DockWindow.OnClose normally calls CloseWindow before the cancelable
        // factory event. Evaluate that event without changing the layout.
        IDockWindow? previous = _evaluatingClose;
        _evaluatingClose = window;
        bool allowed;
        try { allowed = base.OnWindowClosing(window); }
        finally { _evaluatingClose = previous; }
        return allowed && (window is null || closeWindow(window));
    }

    public override void CloseWindow(IDockWindow window)
    {
        if (!ReferenceEquals(_evaluatingClose, window)) base.CloseWindow(window);
    }
}
