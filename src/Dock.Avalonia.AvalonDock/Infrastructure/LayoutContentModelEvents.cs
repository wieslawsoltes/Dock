using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Preserve callback registration identity without retaining discarded controls.
internal sealed class LayoutContentModelEvents(Action changed)
{
    private readonly ConditionalWeakTable<LayoutContent, List<Subscription>> _models = new();

    internal void Add(LayoutContent model)
    {
        List<Subscription> subscriptions = _models.GetValue(model, static _ => new());
        Subscription subscription = new(this);
        subscriptions.Add(subscription);
        model.PropertyChanged += subscription.Changed;
    }

    internal void Remove(LayoutContent? model)
    {
        if (model is null || !_models.TryGetValue(model, out List<Subscription>? subscriptions)) return;
        int last = subscriptions.Count - 1;
        Subscription subscription = subscriptions[last];
        subscriptions.RemoveAt(last);
        if (last == 0) _models.Remove(model);
        model.PropertyChanged -= subscription.Changed;
    }

    private sealed class Subscription(LayoutContentModelEvents owner)
    {
        private readonly WeakReference<LayoutContentModelEvents> _owner = new(owner);

        internal void Changed(object? sender, PropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutContentModelEvents? owner))
            {
                if (sender is LayoutContent model) model.PropertyChanged -= Changed;
                return;
            }
            if (args.PropertyName == nameof(LayoutContent.IsEnabled)) owner.Notify();
        }
    }

    private void Notify() => changed();
}
