using System;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Keep native window vetoes first, then let the compatibility layer close or
// hide content before the native host can detach the floating layout.
internal sealed class LayoutFactory(Func<IDockWindow, bool> closeWindow, ILayoutFloatingPlacement floatingPlacement,
    ILayoutOperationEvents operations, Func<IDockable, LayoutItem?> getLayoutItem,
    Func<IDockable, ILayoutContainer?> getLayoutContainer) : Factory, IDockOperationObserver, IDockWindowHostFactory
{
    private IDockWindow? _evaluatingClose;
    internal IDockWindow? RequestedHostWindow { get; private set; }
    internal event Action? SettingFocus;
    internal event Action<IDock>? FocusSet;

    public override void SetFocusedDockable(IDock dock, IDockable? dockable)
    {
        // Native focus notifications precede activation/selection synchronization.
        // Let the public layout report the completed operation to binding observers.
        SettingFocus?.Invoke();
        try { base.SetFocusedDockable(dock, dockable); }
        finally { FocusSet?.Invoke(dock); }
    }

    public IHostWindow? CreateHostWindow(IDockWindow window)
    {
        // Preserve native ID-based and application-provided locators. The
        // manager's locator can additionally choose a host for this model.
        IDockWindow? previous = RequestedHostWindow;
        RequestedHostWindow = window;
        try { return GetHostWindow(window.Id); }
        finally { RequestedHostWindow = previous; }
    }

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

    public IDockOperationScope? BeginDockOperation(IDockable source, IDockable target, DockOperation operation)
    {
        LayoutOperation notification = operation == DockOperation.Window ? LayoutOperation.Float : LayoutOperation.Dock;
        if (operation == DockOperation.Fill && source is not IDock &&
            (ReferenceEquals(source.Owner, target) || ReferenceEquals(source.Owner, target.Owner)))
            notification = LayoutOperation.Silent;
        return operations.Begin(source, notification);
    }

    public override void FloatAllDockables(IDockable dockable, DockWindowOptions? options)
    {
        using IDockOperationScope? scope = operations.Begin(dockable.Owner ?? dockable, LayoutOperation.Float);
        floatingPlacement.FloatAll(dockable, () => base.FloatAllDockables(dockable, options));
        scope?.Complete();
    }

    public override void FloatDockable(IDockable dockable, DockWindowOptions? options)
    {
        if (dockable.Owner is not IDock || !DockCapabilityResolver.IsEnabled(dockable,
                DockCapability.Float, DockCapabilityResolver.ResolveOperationDock(dockable))) return;
        // Native floating first unpins auto-hidden content. Observe that change
        // within the same scope as the later window creation.
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Float);
        base.FloatDockable(dockable, options);
        scope?.Complete();
    }

    public override void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height, DockWindowOptions? options)
    {
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Float);
        floatingPlacement.FloatContainer(dockable, () => base.SplitToWindow(dock, dockable, x, y, width, height, options));
        scope?.Complete();
    }

    public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        using IDockOperationScope? scope = ReferenceEquals(sourceDock, targetDock)
            ? null : operations.Begin(sourceDockable, LayoutOperation.Dock);
        base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
        scope?.Complete();
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Dock);
        base.SplitToDock(dock, dockable, operation);
        scope?.Complete();
    }
}
