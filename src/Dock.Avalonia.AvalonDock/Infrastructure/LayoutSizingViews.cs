using System;
using System.Collections.Generic;

namespace AvalonDock.Infrastructure;

// Explicit pane dimensions can change without changing the host's bounds.
// Keep only weak registrations, including panels in separate floating hosts.
internal sealed class LayoutSizingViews
{
    private readonly List<WeakReference<LayoutSizingPanel>> _views = new();

    internal void Add(LayoutSizingPanel panel)
    {
        Remove(panel);
        _views.Add(new(panel));
    }

    internal void Remove(LayoutSizingPanel panel)
    {
        for (int i = _views.Count - 1; i >= 0; i--)
            if (!_views[i].TryGetTarget(out LayoutSizingPanel? view) || ReferenceEquals(view, panel)) _views.RemoveAt(i);
    }

    internal void Invalidate()
    {
        for (int i = _views.Count - 1; i >= 0; i--)
            if (_views[i].TryGetTarget(out LayoutSizingPanel? view)) view.InvalidateMeasure();
            else _views.RemoveAt(i);
    }
}
