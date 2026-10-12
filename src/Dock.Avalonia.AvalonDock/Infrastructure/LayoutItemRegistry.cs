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
    private readonly HashSet<LayoutContent> _creating = new();
    private HashSet<LayoutElement> _observed = new();
    private LayoutRoot? _root;
    private bool _queued;
    private bool _refreshing;
    private bool _reconciling;
    private int _generation;
    private int _sourceBatches;

    public LayoutItemRegistry(DockingManager manager) => _manager = manager;

    public LayoutItem? Get(LayoutContent content) => _byContent.GetValueOrDefault(content);

    internal void BeginSourceBatch() => _sourceBatches++;
    internal void EndSourceBatch()
    {
        if (--_sourceBatches != 0 || _root is null) return;
        foreach (ILayoutElement element in _root.Descendents())
            if (element is LayoutElement model) Observe(model);
    }

    internal void CompleteSourceInsertion(LayoutContent content, bool initialImport)
    {
        if ((initialImport || ReferenceEquals(content.Root?.Manager, _manager)) && !_byContent.ContainsKey(content))
            Create(content);
    }

    internal LayoutItem? GetForNativePresentation(LayoutContent content)
    {
        // Native moves can build the destination after removal from the source
        // and before ownership is assigned. Reuse the container retained until
        // reconciliation so its presenter can transfer between native hosts.
        if (content.Root is null && ReferenceEquals(content.DockModel.Factory, _manager.DockFactory) &&
            _byContent.TryGetValue(content, out LayoutItem? existing)) return existing;
        return ReferenceEquals(content.Root, _root) ? Get(content) : null;
    }

    private void Create(LayoutContent content)
    {
        if (!_creating.Add(content)) return;
        LayoutItem item = content switch
        {
            LayoutDocument => new LayoutDocumentItem(),
            LayoutAnchorable => new LayoutAnchorableItem(),
            _ => throw new NotSupportedException("A layout item must be a document or an anchorable.")
        };
        try
        {
            item.Attach(_manager, content);
            _byContent.Add(content, item);
            _items.Add(item);
            if (_observed.Add(content)) content.PropertyChanged += ModelChanged;
            content.NotifyLayoutItemChanged();
        }
        catch
        {
            _byContent.Remove(content);
            _items.Remove(item);
            item.Detach();
            throw;
        }
        finally { _creating.Remove(content); }
    }

    public void SetLayout(LayoutRoot root)
    {
        _generation++;
        _queued = false;
        if (_root is not null)
        {
            _root.Updated -= RootUpdated;
            _root.ElementAdded -= ElementAdded;
            _root.ElementRemoved -= ElementRemoved;
        }
        foreach (LayoutElement element in _observed) element.PropertyChanged -= ModelChanged;
        _observed.Clear();
        _byContent.Clear();
        foreach (LayoutItem item in _items) item.Detach();
        _items.Clear();
        _root = root;
        root.Updated += RootUpdated;
        root.ElementAdded += ElementAdded;
        root.ElementRemoved += ElementRemoved;
        CreateLayoutItems();
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
        RefreshCommands();
    }

    private void Observe(LayoutElement element)
    {
        if (_observed.Add(element)) element.PropertyChanged += ModelChanged;
    }

    private void CreateLayoutItems(LayoutElement? added = null)
    {
        if (_root is null) return;
        // Snapshot before applying application styles, which may edit the tree.
        List<LayoutContent> pending = new();
        foreach (ILayoutElement element in _root.Descendents())
            if (element is LayoutElement model) Collect(model, pending);
        // Native floating roots acquire their public parent before Dock inserts
        // the window into its collection. Register that attaching subtree too.
        if (added is not null)
        {
            Collect(added, pending);
            foreach (ILayoutElement element in added.Descendents())
                if (element is LayoutElement model) Collect(model, pending);
        }
        // Initial layout attachment registers documents before tools. An
        // ordinary membership change follows the tree's current order.
        if (added is null)
            foreach (LayoutContent content in pending)
                if (content is LayoutDocument && !_byContent.ContainsKey(content)) Create(content);
        foreach (LayoutContent content in pending)
            if (!_byContent.ContainsKey(content)) Create(content);
    }

    private void Collect(LayoutElement model, List<LayoutContent> pending)
    {
        Observe(model);
        if (model is LayoutContent content && !_byContent.ContainsKey(content)) pending.Add(content);
    }

    private void ElementAdded(object? sender, LayoutElementEventArgs args)
    {
        if (_sourceBatches != 0) return;
        CreateLayoutItems(args.Element);
        QueueReconcile();
    }

    private void ElementRemoved(object? sender, LayoutElementEventArgs args)
    {
        if (_sourceBatches == 0) QueueReconcile();
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
        else if (e.PropertyName is "Orientation" or "ChildrenCount" or "DockCapabilityPolicy" or
            "RootDockCapabilityPolicy" or "DockCapabilityOverrides" or "CanCloseLastDockable") RefreshCommands();
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
