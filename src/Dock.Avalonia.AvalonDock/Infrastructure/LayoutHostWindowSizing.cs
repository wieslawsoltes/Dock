using System;
using System.Collections.Generic;
using Avalonia;

namespace AvalonDock.Infrastructure;

// Propagate manager settings without keeping closed floating hosts alive.
internal sealed class LayoutHostWindowSizing
{
    private readonly List<WeakReference<LayoutHostWindow>> _hosts = new();
    private double _width;
    private double _height;

    internal LayoutHostWindowSizing(DockingManager manager)
    {
        _width = manager.GridSplitterWidth;
        _height = manager.GridSplitterHeight;
        manager.PropertyChanged += OnManagerChanged;
    }

    internal void Attach(LayoutHostWindow host)
    {
        for (int i = _hosts.Count - 1; i >= 0; i--)
            if (!_hosts[i].TryGetTarget(out _)) _hosts.RemoveAt(i);
        _hosts.Add(new(host));
        Apply(host);
    }

    private void OnManagerChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == DockingManager.GridSplitterWidthProperty) _width = change.GetNewValue<double>();
        else if (change.Property == DockingManager.GridSplitterHeightProperty) _height = change.GetNewValue<double>();
        else return;
        for (int i = _hosts.Count - 1; i >= 0; i--)
            if (_hosts[i].TryGetTarget(out LayoutHostWindow? host)) Apply(host);
            else _hosts.RemoveAt(i);
    }

    private void Apply(LayoutHostWindow host)
    {
        LayoutSplitterSizing.SetWidth(host, _width);
        LayoutSplitterSizing.SetHeight(host, _height);
    }
}
