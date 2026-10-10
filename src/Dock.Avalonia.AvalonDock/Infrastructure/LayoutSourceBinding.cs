using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

internal sealed class LayoutSourceBinding<T> where T : LayoutContent
{
    private readonly DockingManager _manager;
    private readonly Func<object?, T> _create;
    private readonly Action<LayoutRoot, T, ILayoutUpdateStrategy?> _insert;
    private LayoutRoot _layout;
    private IEnumerable? _source;
    private Subscription? _subscription;

    internal LayoutSourceBinding(DockingManager manager, Func<object?, T> create,
        Action<LayoutRoot, T, ILayoutUpdateStrategy?> insert)
    {
        _manager = manager;
        _layout = manager.Layout;
        _create = create;
        _insert = insert;
    }

    internal void SetSource(IEnumerable? source)
    {
        _subscription?.Dispose();
        _subscription = null;
        bool replacing = _source is not null;
        if (replacing) RemoveMatching(_source!, false);
        _source = source;
        try { ImportExistingSource(); }
        finally
        {
            if (source is INotifyCollectionChanged changes) _subscription = new Subscription(changes, this);
        }
        if (replacing) _layout.CollectGarbage();
    }

    internal void SetLayout(LayoutRoot layout)
    {
        if (_source is not null)
        {
            RemoveMatching(_source, false);
            _layout.CollectGarbage();
        }
        _layout = layout;
        ImportExistingSource();
    }

    private void ImportExistingSource()
    {
        if (_source is null) return;
        HashSet<object?> existing = new();
        foreach (ILayoutElement element in _layout.Descendents())
            if (element is T content) existing.Add(content.Content);
        foreach (object? item in _source)
        {
            if (item is null || existing.Contains(item)) continue;
            T content = _create(item);
            content.ContentId = Guid.NewGuid().ToString();
            _insert(_layout, content, _manager.LayoutUpdateStrategy);
        }
    }

    private void RemoveMatching(IEnumerable items, bool clearContent)
    {
        HashSet<object?> removed = new();
        foreach (object? item in items) removed.Add(item);
        List<T> contents = new();
        foreach (ILayoutElement element in _layout.Descendents())
            if (element is T content && removed.Contains(content.Content)) contents.Add(content);
        foreach (T content in contents)
        {
            content.Parent?.RemoveChild(content);
            if (clearContent) content.Content = null;
        }
    }

    private void SourceChanged(NotifyCollectionChangedEventArgs args)
    {
        if (args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace && args.OldItems is { } removed)
            RemoveMatching(removed, true);
        if (args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace && args.NewItems is { } added)
            foreach (object? item in added) _insert(_layout, _create(item), _manager.LayoutUpdateStrategy);
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            // AvalonDock resets every item of the source's layout type,
            // including items inserted directly into the layout.
            List<T> contents = new();
            foreach (ILayoutElement element in _layout.Descendents()) if (element is T content) contents.Add(content);
            foreach (T content in contents)
            {
                content.Parent?.RemoveChild(content);
                if (content is LayoutDocument) content.Content = null;
            }
        }
        _layout.CollectGarbage();
    }

    // The application's collection may outlive its manager. Its subscription
    // must not retain an otherwise unused manager or layout.
    private sealed class Subscription : IDisposable
    {
        private readonly INotifyCollectionChanged _source;
        private readonly WeakReference<LayoutSourceBinding<T>> _target;
        internal Subscription(INotifyCollectionChanged source, LayoutSourceBinding<T> target)
        {
            _source = source;
            _target = new(target);
            source.CollectionChanged += Changed;
        }
        private void Changed(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (_target.TryGetTarget(out LayoutSourceBinding<T>? target)) target.SourceChanged(args);
            else Dispose();
        }
        public void Dispose() => _source.CollectionChanged -= Changed;
    }
}
