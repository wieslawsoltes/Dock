using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutSourceBinding<T> where T : LayoutContent
{
    private readonly DockingManager _manager;
    private readonly LayoutItemRegistry _items;
    private readonly Func<object?, T> _create;
    private readonly Func<LayoutRoot, ILayoutContainer?> _findDestination;
    private readonly Func<LayoutRoot?, T, ILayoutContainer?, DockingManager?, ILayoutContainer?> _insert;
    private readonly LayoutSourceSubscriptions _subscriptions;

    internal LayoutSourceBinding(DockingManager manager, LayoutItemRegistry items, Func<object?, T> create,
        Func<LayoutRoot, ILayoutContainer?> findDestination,
        Func<LayoutRoot?, T, ILayoutContainer?, DockingManager?, ILayoutContainer?> insert,
        LayoutSourceSubscriptions subscriptions)
    {
        _manager = manager;
        _items = items;
        _create = create;
        _findDestination = findDestination;
        _insert = insert;
        _subscriptions = subscriptions;
    }

    internal void ChangeSource(IEnumerable? oldSource, IEnumerable? newSource)
    {
        Detach(_manager.Layout, oldSource);
        Attach(_manager.Layout, newSource);
    }

    internal void Detach(LayoutRoot? layout, IEnumerable? source)
    {
        if (layout is null || source is null) return;
        RemoveMatching(layout, source, false);
        _subscriptions.Detach(source);
        _manager.Layout.CollectGarbage();
    }

    internal void Attach(LayoutRoot? layout, IEnumerable? source)
    {
        if (layout is null || source is null) return;
        HashSet<object?> existing = new();
        foreach (ILayoutElement element in layout.Descendents())
            if (element is T content) existing.Add(content.Content);
        // Snapshot before invoking application strategies, which may edit the source.
        // Repeated source values remain distinct unless already present in the layout.
        List<object> pending = new();
        foreach (object? item in source)
            if (item is not null && !existing.Contains(item)) pending.Add(item);
        InsertItems(layout, pending, true);
        // Only a completed import attaches its supplied source. A strategy may
        // have changed the manager's current source while this batch ran.
        _subscriptions.Attach(source);
    }

    private void InsertItems(LayoutRoot layout, IEnumerable items, bool assignContentIds)
    {
        ILayoutContainer? destination = _findDestination(layout);
        _items.BeginSourceBatch();
        try
        {
            foreach (object? item in items)
            {
                T content = _create(item);
                if (assignContentIds) content.ContentId = Guid.NewGuid().ToString();
                // Initial attachment imports into its supplied root, while live
                // collection notifications consult the manager's current layout.
                destination = _insert(assignContentIds ? layout : null, content, destination, _manager);
                _items.CompleteSourceInsertion(content, assignContentIds);
            }
        }
        finally { _items.EndSourceBatch(); }
    }

    private static void RemoveMatching(LayoutRoot layout, IEnumerable items, bool clearContent)
    {
        HashSet<object?> removed = new();
        foreach (object? item in items) removed.Add(item);
        List<T> contents = new();
        foreach (ILayoutElement element in layout.Descendents())
            if (element is T content && removed.Contains(content.Content)) contents.Add(content);
        foreach (T content in contents)
        {
            content.Parent?.RemoveChild(content);
            if (clearContent) content.Content = null;
        }
    }

    internal void SourceChanged(NotifyCollectionChangedEventArgs args)
    {
        if (_manager.AreSourceBindingsSuspended) return;
        if (args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace && args.OldItems is { } removed)
            RemoveMatching(_manager.Layout, removed, true);
        if (args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace && args.NewItems is { } added)
            InsertItems(_manager.Layout, added, false);
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            // AvalonDock resets every item of the source's layout type,
            // including items inserted directly into the layout.
            List<T> contents = new();
            foreach (ILayoutElement element in _manager.Layout.Descendents()) if (element is T content) contents.Add(content);
            foreach (T content in contents)
            {
                content.Parent?.RemoveChild(content);
                if (content is LayoutDocument) content.Content = null;
            }
        }
        _manager.Layout.CollectGarbage();
    }
}
