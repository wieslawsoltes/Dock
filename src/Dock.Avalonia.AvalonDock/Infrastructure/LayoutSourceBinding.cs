using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutSourceBinding<T> where T : LayoutContent
{
    private readonly DockingManager _manager;
    private readonly Func<object?, T> _create;
    private readonly Func<LayoutRoot, ILayoutContainer?> _findDestination;
    private readonly Func<LayoutRoot?, T, ILayoutContainer?, DockingManager?, ILayoutContainer?> _insert;
    private readonly Action<IEnumerable?> _observe;
    private LayoutRoot _layout;
    private IEnumerable? _source;

    internal LayoutSourceBinding(DockingManager manager, Func<object?, T> create,
        Func<LayoutRoot, ILayoutContainer?> findDestination,
        Func<LayoutRoot?, T, ILayoutContainer?, DockingManager?, ILayoutContainer?> insert,
        Action<IEnumerable?> observe)
    {
        _manager = manager;
        _layout = manager.Layout;
        _create = create;
        _findDestination = findDestination;
        _insert = insert;
        _observe = observe;
    }

    internal void SetSource(IEnumerable? source)
    {
        _observe(null);
        bool replacing = _source is not null;
        if (replacing) RemoveMatching(_source!, false);
        _source = source;
        try { ImportExistingSource(); }
        finally { _observe(_source); }
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
        // Snapshot before invoking application strategies, which may edit the source.
        // Repeated source values remain distinct unless already present in the layout.
        List<object> pending = new();
        foreach (object? item in _source)
            if (item is not null && !existing.Contains(item)) pending.Add(item);
        InsertItems(pending, true);
    }

    private void InsertItems(IEnumerable items, bool assignContentIds)
    {
        LayoutRoot importedLayout = _layout;
        ILayoutContainer? destination = _findDestination(_layout);
        foreach (object? item in items)
        {
            T content = _create(item);
            if (assignContentIds) content.ContentId = Guid.NewGuid().ToString();
            // Initial attachment imports into its supplied root, while live
            // collection notifications consult the manager's current layout.
            destination = _insert(assignContentIds ? importedLayout : null, content, destination, _manager);
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

    internal void SourceChanged(NotifyCollectionChangedEventArgs args)
    {
        if (_manager.AreSourceBindingsSuspended) return;
        if (args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace && args.OldItems is { } removed)
            RemoveMatching(removed, true);
        if (args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace && args.NewItems is { } added)
            InsertItems(added, false);
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
}
