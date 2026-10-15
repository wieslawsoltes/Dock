using System.Collections.Generic;
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
        foreach (LayoutFloatingWindow window in new List<LayoutFloatingWindow>(layout.FloatingWindows)) Present(window);
    }

    internal void Present(LayoutFloatingWindow window)
    {
        if (!ReferenceEquals(window.Root, _layout) || _layout is null) return;
        if (!window.IsValid)
        {
            // Compatible hosts observe their model's public notifications.
            // A model override may intentionally suppress those notifications.
            if (window.DockWindow.Host is not LayoutFloatingWindowControl) window.CloseHost();
        }
        else if (window is LayoutAnchorableFloatingWindow { IsVisible: false } or LayoutGroupFloatingWindow { IsVisible: false })
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

    internal void Detach(bool internalClose = true, IEnumerable<LayoutFloatingWindowControl>? registeredHosts = null)
    {
        if (_layout is null) return;
        LayoutRoot layout = _layout;
        _layout = null;
        List<LayoutFloatingWindowControl> hosts = registeredHosts is null ? new() : new(registeredHosts);
        foreach (LayoutFloatingWindow window in new List<LayoutFloatingWindow>(layout.FloatingWindows))
        {
            if (window.DockWindow.Host is LayoutFloatingWindowControl host)
            {
                if (!hosts.Contains(host)) hosts.Add(host);
            }
            else window.CloseHost(internalClose);
        }
        foreach (LayoutFloatingWindowControl host in hosts) host.RequestCloseFromLayout(internalClose);
    }
}
