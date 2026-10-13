using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Callback arguments control these registrations independently of the root
// currently presented by the native controls. Preserve multicast ordering and
// remove one matching registration without retaining either endpoint.
internal sealed class LayoutRootEventSubscriptions(DockingManager manager)
{
    private readonly ConditionalWeakTable<LayoutRoot, List<Subscription>> _roots = new();

    internal void Attach(LayoutRoot? root)
    {
        if (root is null) return;
        List<Subscription> subscriptions = _roots.GetValue(root, static _ => new());
        Subscription subscription = new(manager);
        subscriptions.Add(subscription);
        root.PropertyChanged += subscription.Changed;
        root.Updated += subscription.Updated;
    }

    internal void Detach(LayoutRoot? root)
    {
        if (root is null || !_roots.TryGetValue(root, out List<Subscription>? subscriptions)) return;
        int last = subscriptions.Count - 1;
        Subscription subscription = subscriptions[last];
        subscriptions.RemoveAt(last);
        if (last == 0) _roots.Remove(root);
        subscription.Detach(root);
    }

    private sealed class Subscription(DockingManager manager)
    {
        private readonly WeakReference<DockingManager> _manager = new(manager);

        internal void Changed(object? sender, PropertyChangedEventArgs args)
        {
            if (_manager.TryGetTarget(out DockingManager? manager)) manager.LayoutPropertyChanged(args);
            else if (sender is LayoutRoot root) Detach(root);
        }

        internal void Updated(object? sender, EventArgs args)
        {
            if (_manager.TryGetTarget(out DockingManager? manager)) manager.LayoutRootUpdated();
            else if (sender is LayoutRoot root) Detach(root);
        }

        internal void Detach(LayoutRoot root)
        {
            root.PropertyChanged -= Changed;
            root.Updated -= Updated;
        }
    }
}
