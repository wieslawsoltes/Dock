using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Keep native window vetoes first, then let the compatibility layer close or
// hide content before the native host can detach the floating layout.
internal sealed class LayoutFactory(Func<IDockWindow, bool> closeWindow, ILayoutFloatingPlacement floatingPlacement,
    Func<IDockable, LayoutItem?> getLayoutItem, Func<IDockable, ILayoutContainer?> getLayoutContainer) : Factory
{
    private IDockWindow? _evaluatingClose;

    internal LayoutItem? GetLayoutItem(IDockable model) => getLayoutItem(model);
    internal ILayoutContainer? GetLayoutContainer(IDockable model) => getLayoutContainer(model);

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

    public override void FloatAllDockables(IDockable dockable, DockWindowOptions? options) =>
        floatingPlacement.FloatAll(dockable, () => base.FloatAllDockables(dockable, options));

    public override void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height, DockWindowOptions? options) =>
        floatingPlacement.FloatContainer(dockable, () => base.SplitToWindow(dock, dockable, x, y, width, height, options));
}
