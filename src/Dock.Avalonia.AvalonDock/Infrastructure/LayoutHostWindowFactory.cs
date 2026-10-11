using System;
using Avalonia;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHostWindowFactory(DockingManager manager)
{
    public static readonly AttachedProperty<Func<IHostWindow?>?> FactoryProperty =
        AvaloniaProperty.RegisterAttached<LayoutHostWindowFactory, StyledElement, Func<IHostWindow?>?>("Factory", inherits: true);

    public static Func<IHostWindow?>? GetFactory(StyledElement element) => element.GetValue(FactoryProperty);
    public static void SetFactory(StyledElement element, Func<IHostWindow?>? value) => element.SetValue(FactoryProperty, value);

    private readonly WeakReference<DockingManager> _manager = new(manager);
    private readonly LayoutHostWindowSizing _sizing = new(manager);

    internal IHostWindow? Create()
    {
        if (!_manager.TryGetTarget(out DockingManager? owner)) return null;
        HostWindow window;
        if (owner.DockFactory is LayoutFactory { RequestedHostWindow: { } model })
            return Create(owner.Layout.Context.GetWindow(model));
        else window = new LayoutHostWindow();
        Configure(owner, window);
        return window;
    }

    internal LayoutFloatingWindowControl Create(LayoutFloatingWindow model)
    {
        if (!_manager.TryGetTarget(out DockingManager? owner)) throw new InvalidOperationException("The manager is no longer available.");
        LayoutFloatingWindowControl window = model is LayoutDocumentFloatingWindow document
            ? new LayoutDocumentFloatingWindowControl(document)
            : new LayoutAnchorableFloatingWindowControl((LayoutAnchorableFloatingWindow)model);
        window.Window = model.DockWindow;
        owner.FloatingWindowCreation.Configure(model, window);
        Configure(owner, window);
        return window;
    }

    private void Configure(DockingManager owner, HostWindow window)
    {
        owner.AttachThemeResources(window);
        SetFactory(window, owner.DockHostWindowFactory);
        window.DataTemplates.Add(new LayoutScopedDataTemplate(owner));
        _sizing.Attach(window);
    }
}
