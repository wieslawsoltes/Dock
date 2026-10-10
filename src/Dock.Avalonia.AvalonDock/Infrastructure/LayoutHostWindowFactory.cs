using System;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHostWindowFactory(DockingManager manager)
{
    private readonly WeakReference<DockingManager> _manager = new(manager);

    internal IHostWindow? Create()
    {
        if (!_manager.TryGetTarget(out DockingManager? owner)) return null;
        LayoutHostWindow window = new();
        window.DataTemplates.Add(new LayoutScopedDataTemplate(owner));
        return window;
    }
}
