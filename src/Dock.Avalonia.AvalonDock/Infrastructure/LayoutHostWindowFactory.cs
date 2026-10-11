using System;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHostWindowFactory(DockingManager manager)
{
    private readonly WeakReference<DockingManager> _manager = new(manager);
    private readonly LayoutHostWindowSizing _sizing = new(manager);

    internal IHostWindow? Create()
    {
        if (!_manager.TryGetTarget(out DockingManager? owner)) return null;
        HostWindow window;
        if (owner.DockFactory is LayoutFactory { RequestedHostWindow: { } model })
        {
            LayoutFloatingWindow floating = owner.Layout.Context.GetWindow(model);
            window = floating is LayoutDocumentFloatingWindow document
                ? new LayoutDocumentFloatingWindowControl(document)
                : new LayoutAnchorableFloatingWindowControl((LayoutAnchorableFloatingWindow)floating);
            window.Window = model;
        }
        else window = new LayoutHostWindow();
        window.DataTemplates.Add(new LayoutScopedDataTemplate(owner));
        _sizing.Attach(window);
        return window;
    }
}
