using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia.Utilities;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutSourceSubscriptions(IWeakEventSubscriber<NotifyCollectionChangedEventArgs> listener)
{
    private readonly ConditionalWeakTable<INotifyCollectionChanged, List<Subscription>> _subscriptions = new();

    internal void Attach(IEnumerable source)
    {
        if (source is not INotifyCollectionChanged notifier) return;
        List<Subscription> subscriptions = _subscriptions.GetValue(notifier, static _ => new());
        // Each attachment is distinct, including shared sources and callback
        // replays. Weak keys and weak event listeners retain neither endpoint.
        Subscription subscription = new(listener);
        subscriptions.Add(subscription);
        WeakEvents.CollectionChanged.Subscribe(notifier, subscription);
    }

    internal void Detach(IEnumerable source)
    {
        if (source is not INotifyCollectionChanged notifier ||
            !_subscriptions.TryGetValue(notifier, out List<Subscription>? subscriptions)) return;
        int last = subscriptions.Count - 1;
        Subscription subscription = subscriptions[last];
        subscriptions.RemoveAt(last);
        if (last == 0) _subscriptions.Remove(notifier);
        WeakEvents.CollectionChanged.Unsubscribe(notifier, subscription);
    }

    private sealed class Subscription(IWeakEventSubscriber<NotifyCollectionChangedEventArgs> subscriber)
        : IWeakEventSubscriber<NotifyCollectionChangedEventArgs>
    {
        private readonly WeakReference<IWeakEventSubscriber<NotifyCollectionChangedEventArgs>> _subscriber = new(subscriber);

        public void OnEvent(object? sender, WeakEvent ev, NotifyCollectionChangedEventArgs e)
        {
            if (_subscriber.TryGetTarget(out IWeakEventSubscriber<NotifyCollectionChangedEventArgs>? target))
                target.OnEvent(sender, ev, e);
        }
    }
}
