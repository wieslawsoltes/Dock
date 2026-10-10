using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Containers follow model identity, including temporary removal during native
// moves. Reconcile on the dispatcher after a tree mutation has finished.
internal sealed class LayoutItemRegistry
{
    private readonly DockingManager _manager;
    private readonly Dictionary<LayoutContent, LayoutItem> _byContent = new();
    private readonly List<LayoutItem> _items = new();
    private HashSet<LayoutElement> _observed = new();
    private LayoutRoot? _root;
    private bool _queued;
    private bool _refreshing;
    private bool _reconciling;
    private int _generation;

    public LayoutItemRegistry(DockingManager manager) => _manager = manager;

    public LayoutItem? Get(LayoutContent content)
    {
        if (!ReferenceEquals(content.Root, _root)) return null;
        if (_byContent.TryGetValue(content, out LayoutItem? existing)) return existing;
        LayoutItem item = Create(content);
        Reconcile();
        return item;
    }

    private LayoutItem Create(LayoutContent content)
    {
        LayoutItem item = content switch
        {
            LayoutDocument => new LayoutDocumentItem(),
            LayoutAnchorable => new LayoutAnchorableItem(),
            _ => throw new NotSupportedException("A layout item must be a document or an anchorable.")
        };
        _byContent.Add(content, item);
        _items.Add(item);
        item.Attach(_manager, content);
        return item;
    }

    public void SetLayout(LayoutRoot root)
    {
        _generation++;
        _queued = false;
        if (_root is not null) _root.Updated -= RootUpdated;
        foreach (LayoutElement element in _observed) element.PropertyChanged -= ModelChanged;
        _observed.Clear();
        _byContent.Clear();
        foreach (LayoutItem item in _items) item.Detach();
        _items.Clear();
        _root = root;
        root.Updated += RootUpdated;
        Reconcile();
    }

    public void RefreshCommands()
    {
        if (_refreshing) return;
        _refreshing = true;
        try
        {
            for (int i = 0; i < _items.Count; i++) _items[i].RefreshCommands();
        }
        finally { _refreshing = false; }
    }

    public void RefreshTemplates()
    {
        for (int i = 0; i < _items.Count; i++) _items[i].RefreshViewTemplate();
    }

    public void RefreshHeaders(LayoutHeaderKind kind)
    {
        for (int i = 0; i < _items.Count; i++) _items[i].RefreshHeaders(kind);
    }

    public void RefreshStyles()
    {
        Reconcile();
        for (int i = 0; i < _items.Count; i++) _items[i].ApplyContainerStyle();
    }

    private void Reconcile()
    {
        if (_root is null || _reconciling) return;
        _reconciling = true;
        try { ReconcileCore(); }
        finally { _reconciling = false; }
    }

    private void ReconcileCore()
    {
        if (_root is null) return;
        HashSet<LayoutElement> current = new() { _root };
        foreach (ILayoutElement element in _root.Descendents())
            if (element is LayoutElement model) current.Add(model);
        foreach (LayoutElement element in _observed)
            if (!current.Contains(element)) element.PropertyChanged -= ModelChanged;
        foreach (LayoutElement element in current)
            if (!_observed.Contains(element)) element.PropertyChanged += ModelChanged;
        _observed = current;

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            LayoutItem item = _items[i];
            if (item.LayoutElement is not { } content || current.Contains(content)) continue;
            _byContent.Remove(content);
            _items.RemoveAt(i);
            item.Detach();
        }
        if (_manager.LayoutItemContainerStyle is not null || _manager.LayoutItemContainerStyleSelector is not null)
            foreach (LayoutElement element in current)
                if (element is LayoutContent content && ReferenceEquals(content.Root, _root) && !_byContent.ContainsKey(content))
                    Create(content);
        RefreshCommands();
    }

    private void RootUpdated(object? sender, EventArgs e)
    {
        QueueReconcile();
        RefreshCommands();
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is LayoutContent content && _byContent.TryGetValue(content, out LayoutItem? item))
        {
            if (e.PropertyName is "Title" or "ContentId" or "Id" or "Content" or "IconSource" or "ToolTip" or
                "IsSelected" or "IsActive" or "CanClose" or "CanFloat" or "CanHide" or "Description" or
                "IsVisible" or "Parent" or "Root") item.SynchronizeModel(e.PropertyName);
            if (e.PropertyName is "CanClose" or "CanFloat" or "CanHide" or "CanDrag" or "CanMove" or
                "CanPin" or "CanAutoHide" or "CanDockAsDocument" or "CanDockAsTabbedDocument" or
                "DockingState" or "IsFloating" or "IsHidden") item.RefreshCommands();
        }
        if (e.PropertyName is nameof(LayoutElement.Parent) or nameof(LayoutElement.Root))
        {
            QueueReconcile();
            RefreshCommands();
        }
        else if (e.PropertyName is "Orientation" or "ChildrenCount") RefreshCommands();
    }

    private void QueueReconcile()
    {
        if (_queued) return;
        _queued = true;
        int generation = _generation;
        WeakReference<LayoutItemRegistry> reference = new(this);
        Dispatcher.UIThread.Post(() =>
        {
            if (!reference.TryGetTarget(out LayoutItemRegistry? registry) || registry._generation != generation) return;
            registry._queued = false;
            registry.Reconcile();
        }, DispatcherPriority.Background);
    }
}
