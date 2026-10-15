using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;
using Dock.Avalonia.Controls;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutOverlayWindowFactory(DockingManager manager) : IDockAdornerWindowFactory
{
    private readonly WeakReference<DockingManager> _manager = new(manager);

    public DockAdornerWindow? CreateWindow(Visual adornedElement, Control adorner) =>
        _manager.TryGetTarget(out DockingManager? owner) && adornedElement is Control surface
            ? new OverlayWindow(owner, surface, adorner) : null;
}
