using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

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
        if (!window.IsValid || window is LayoutAnchorableFloatingWindow { IsVisible: false })
            window.CloseHost();
        else if (window.DockWindow.Host?.IsTracked != true)
            window.DockWindow.Present(window.DockWindow.IsModal);
    }

    internal void Detach()
    {
        if (_layout is null) return;
        foreach (LayoutFloatingWindow window in _layout.FloatingWindows) window.CloseHost();
        _layout = null;
    }
}
