using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AvalonDock.Infrastructure;

// A dictionary has one Avalonia owner. Share its lookup and change notifications
// through separate providers, without making the dictionary retain any consumer.
internal sealed class LayoutThemeDictionary
{
    private static readonly AttachedProperty<LayoutThemeDictionary?> SharedProperty =
        AvaloniaProperty.RegisterAttached<LayoutThemeDictionary, ResourceDictionary, LayoutThemeDictionary?>("SharedThemeResources");

    private readonly ResourceDictionary _dictionary;
    private readonly StyledElement _holder = new();
    private readonly List<WeakReference<Provider>> _providers = new();
    private IResourceHost? _observedOwner;
    private bool _observing;
    private ResourcesChangedEventArgs? _lastChange;

    private LayoutThemeDictionary(ResourceDictionary dictionary) => _dictionary = dictionary;

    internal static IResourceProvider CreateProvider(ResourceDictionary dictionary)
    {
        LayoutThemeDictionary? shared = dictionary.GetValue(SharedProperty);
        if (shared is null)
        {
            shared = new(dictionary);
            dictionary.SetValue(SharedProperty, shared);
        }
        return new Provider(shared);
    }

    private void Attach(Provider provider)
    {
        Prune(null);
        _providers.Add(new(provider));
        if (!_observing)
        {
            _observing = true;
            _dictionary.OwnerChanged += OwnerChanged;
        }
        EnsureOwner();
    }

    private void Detach(Provider provider)
    {
        Prune(provider);
        if (_providers.Count != 0) return;
        _observing = false;
        _dictionary.OwnerChanged -= OwnerChanged;
        ObserveOwner(null);
        if (ReferenceEquals(_dictionary.Owner, _holder)) _holder.Resources = new ResourceDictionary();
    }

    private void Prune(Provider? removed)
    {
        for (int index = _providers.Count - 1; index >= 0; index--)
            if (!_providers[index].TryGetTarget(out Provider? provider) || ReferenceEquals(provider, removed))
                _providers.RemoveAt(index);
    }

    private void EnsureOwner()
    {
        if (!_observing) return;
        if (_dictionary.Owner is null) _holder.Resources = _dictionary;
        ObserveOwner(_dictionary.Owner);
    }

    private void OwnerChanged(object? sender, EventArgs args)
    {
        ObserveOwner(_dictionary.Owner);
        if (_dictionary.Owner is null)
        {
            // Let a synchronous transfer to another external owner finish.
            Dispatcher.UIThread.Post(EnsureOwner);
        }
        NotifyProviders(ResourcesChangedEventArgs.Create());
    }

    private void ObserveOwner(IResourceHost? owner)
    {
        if (ReferenceEquals(_observedOwner, owner)) return;
        if (_observedOwner is not null) _observedOwner.ResourcesChanged -= ResourcesChanged;
        _observedOwner = owner;
        if (_observedOwner is not null) _observedOwner.ResourcesChanged += ResourcesChanged;
    }

    private void ResourcesChanged(object? sender, ResourcesChangedEventArgs args) => NotifyProviders(args);

    private void NotifyProviders(ResourcesChangedEventArgs args)
    {
        if (_lastChange == args) return;
        _lastChange = args;
        // Keep Avalonia's notification identity, including when the dictionary's
        // existing owner also consumes it. Resource callbacks may replace themes.
        WeakReference<Provider>[] providers = _providers.ToArray();
        foreach (WeakReference<Provider> reference in providers)
        {
            if (reference.TryGetTarget(out Provider? provider)) provider.Invalidate(args);
            if (_lastChange != args) break;
        }
        Prune(null);
    }

    private sealed class Provider(LayoutThemeDictionary source) : ResourceProvider
    {
        public override bool HasResources => source._dictionary.HasResources;
        public override bool TryGetResource(object key, ThemeVariant? theme, out object? value) =>
            source._dictionary.TryGetResource(key, theme, out value);
        internal void Invalidate(ResourcesChangedEventArgs args) => Owner?.NotifyHostedResourcesChanged(args);
        protected override void OnAddOwner(IResourceHost owner)
        {
            source.Attach(this);
            base.OnAddOwner(owner);
        }
        protected override void OnRemoveOwner(IResourceHost owner)
        {
            source.Detach(this);
            base.OnRemoveOwner(owner);
        }
    }
}
