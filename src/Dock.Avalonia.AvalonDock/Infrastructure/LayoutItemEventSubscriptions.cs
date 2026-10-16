using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal interface ILayoutElementEventListener
{
    void ElementAdded(LayoutElementEventArgs args);
    void ElementRemoved();
}

internal sealed class LayoutItemEventSubscriptions(ILayoutElementEventListener listener)
{
    private readonly ConditionalWeakTable<LayoutRoot, List<Subscription>> _roots = new();

    internal void Attach(LayoutRoot root)
    {
        List<Subscription> subscriptions = _roots.GetValue(root, static _ => new());
        Subscription subscription = new(listener);
        subscriptions.Add(subscription);
        root.ElementAdded += subscription.Added;
        root.ElementRemoved += subscription.Removed;
    }

    internal void Detach(LayoutRoot root)
    {
        if (!_roots.TryGetValue(root, out List<Subscription>? subscriptions)) return;
        int last = subscriptions.Count - 1;
        Subscription subscription = subscriptions[last];
        subscriptions.RemoveAt(last);
        if (last == 0) _roots.Remove(root);
        subscription.Detach(root);
    }

    private sealed class Subscription(ILayoutElementEventListener listener)
    {
        private readonly WeakReference<ILayoutElementEventListener> _listener = new(listener);

        internal void Added(object? sender, LayoutElementEventArgs args)
        {
            if (_listener.TryGetTarget(out ILayoutElementEventListener? listener)) listener.ElementAdded(args);
            else if (sender is LayoutRoot root) Detach(root);
        }

        internal void Removed(object? sender, LayoutElementEventArgs args)
        {
            if (_listener.TryGetTarget(out ILayoutElementEventListener? listener)) listener.ElementRemoved();
            else if (sender is LayoutRoot root) Detach(root);
        }

        internal void Detach(LayoutRoot root)
        {
            root.ElementAdded -= Added;
            root.ElementRemoved -= Removed;
        }
    }
}
