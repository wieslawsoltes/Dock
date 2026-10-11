using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// A manager owns host visibility; detaching a view must preserve its layout.
internal sealed class LayoutFloatingWindowPresenter
{
    private LayoutRoot? _layout;

    internal void Attach(LayoutRoot layout)
    {
        if (!ReferenceEquals(_layout, layout)) Detach();
        _layout = layout;
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) Present(window);
    }

    internal void Present(LayoutFloatingWindow window)
    {
        if (!ReferenceEquals(window.Root, _layout) || _layout is null) return;
        if (!window.IsValid)
            window.CloseHost();
        else if (window is LayoutAnchorableFloatingWindow { IsVisible: false })
        {
            if (window.DockWindow.Host is null)
                window.DockWindow.Factory?.InitDockWindow(window.DockWindow, window.DockWindow.Owner);
            if (window.DockWindow.Host is LayoutFloatingWindowControl { IsTracked: false })
                window.DockWindow.Present(window.DockWindow.IsModal);
            if (window.DockWindow.Host is LayoutFloatingWindowControl host) host.HideForLayout();
            else window.CloseHost();
        }
        else if (window.DockWindow.Host is LayoutFloatingWindowControl { IsHiddenByLayout: true } hidden)
            hidden.RestoreForLayout();
        else if (window.DockWindow.Host?.IsTracked != true)
            window.DockWindow.Present(window.DockWindow.IsModal);
    }

    internal void Detach(bool internalClose = true)
    {
        if (_layout is null) return;
        foreach (LayoutFloatingWindow window in _layout.FloatingWindows) window.CloseHost(internalClose);
        _layout = null;
    }
}
