using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutChildren<T> : ObservableCollection<T>, ILayoutChildrenNotification where T : class, ILayoutElement
{
    private readonly LayoutElement _owner;
    private readonly DockBase _model;
    private readonly Func<IList<IDockable>?> _getSource;
    private readonly Action<IList<IDockable>> _setSource;
    private readonly bool _visible;
    private readonly Action? _notifyOwner;
    private IList<IDockable> _source = null!;
    private bool _syncing;
    private readonly HashSet<T> _unprojectedChildren = new();
    private LayoutElement? _pendingAttachment;
    internal bool IsRefreshingSource { get; private set; }
    internal bool IsMovingSource { get; private set; }
    public bool IsNotifying { get; private set; }
    public event Action? NotificationCompleted;

    internal LayoutChildren(LayoutElement owner, DockBase model,
        Func<IList<IDockable>?>? getSource = null, Action<IList<IDockable>>? setSource = null, Action? notifyOwner = null)
    {
        _owner = owner;
        _model = model;
        _visible = getSource is null;
        _notifyOwner = notifyOwner;
        _getSource = getSource ?? (() => model.VisibleDockables);
        _setSource = setSource ?? (value => model.VisibleDockables = value);
        Reconnect();
        CollectionChanged += ApplyCollectionChange;
    }

    internal void Reconnect()
    {
        if (_source is INotifyCollectionChanged old) old.CollectionChanged -= SourceChanged;
        _source = _getSource() ?? new AvaloniaList<IDockable>();
        if (_getSource() is null) _setSource(_source);
        if (_source is INotifyCollectionChanged current) current.CollectionChanged += SourceChanged;
        Refresh();
    }

    // Auto-hide grouping is metadata over Dock's flat pinned lists. Updating
    // that projection must not change native ownership during a Dock operation.
    internal void AddProjected(T item)
    {
        LayoutElement element = (LayoutElement)(ILayoutElement)item;
        _owner.Context.Adopt(element.Context);
        _source.Add(element.DockModel);
    }

    internal void RemoveProjected(T item) => _source.Remove(((LayoutElement)(ILayoutElement)item).DockModel);

    private void SourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncing) return;
        if (_visible && _owner is ILayoutGroup group)
            for (int i = 0; i < Count; i++)
                if (this[i] is LayoutContent content && !_source.Contains(content.DockModel))
                    content.DockingHistory.CaptureRemoval(group, i);
        Refresh();
    }

    private void Refresh()
    {
        _syncing = true;
        IsRefreshingSource = true;
        try
        {
            // The public ObservableCollection permits repeated references. Dock
            // renders a model once; extra occurrences and the stale occurrence
            // left by a public removal remain part of the facade collection.
            List<(int Index, T Child)> retained = new();
            HashSet<T> seen = new();
            for (int i = 0; i < Count; i++)
            {
                T child = this[i];
                bool inSource = _source.Contains(((LayoutElement)(ILayoutElement)child).DockModel);
                if (inSource) _unprojectedChildren.Remove(child);
                if (!seen.Add(child) || _unprojectedChildren.Contains(child))
                {
                    retained.Add((i, child));
                    if (!inSource) _unprojectedChildren.Add(child);
                }
            }
            List<T> desired = new(_source.Count + retained.Count);
            foreach (IDockable model in _source)
            {
                if (model is ISplitter) continue;
                T child = (T)(ILayoutElement)_owner.Context.GetChild(model, _owner);
                if (!desired.Contains(child)) desired.Add(child);
            }
            foreach ((int index, T child) in retained) desired.Insert(Math.Min(index, desired.Count), child);
            for (int target = 0; target < desired.Count; target++)
            {
                T child = desired[target];
                if (target < Count && ReferenceEquals(this[target], child)) continue;
                int existing = -1;
                for (int i = target + 1; i < Count; i++)
                    if (ReferenceEquals(this[i], child)) { existing = i; break; }
                if (existing >= 0) base.MoveItem(existing, target);
                else base.InsertItem(target, child);
            }
            while (Count > desired.Count) base.RemoveItem(Count - 1);
            _unprojectedChildren.RemoveWhere(child => !Contains(child));
        }
        finally { IsRefreshingSource = false; _syncing = false; }
    }

    private LayoutElement Prepare(T item)
    {
        _owner.VerifyAccess();
        if (item is not LayoutElement element) throw new ArgumentException("Children must use Dock-backed layout elements.", nameof(item));
        for (ILayoutElement? ancestor = _owner; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, item)) throw new InvalidOperationException("A layout cannot contain itself or an ancestor.");
        if (_owner is LayoutAnchorGroup && element is LayoutContent content) content.RememberDockedLocation();
        return element;
    }

    private void Attach(LayoutElement element)
    {
        // The target collection already contains this item when parent hooks
        // run. Use its current parent, including changes made by earlier hooks.
        if (!ReferenceEquals(element.Parent, _owner)) LayoutParenting.RemoveFromParent(element);
        _owner.Context.Adopt(element.Context);
        if (_model.Factory is { } factory) factory.InitDockable(element.DockModel, _model);
        else element.DockModel.Owner = _model;
    }

    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
        LayoutElement element = Prepare(item);
        Mutate(() =>
        {
            if (!_source.Contains(element.DockModel)) _source.Insert(GetNativeInsertionIndex(index), element.DockModel);
            _unprojectedChildren.Remove(item);
            LayoutElement? previousAttachment = _pendingAttachment;
            _pendingAttachment = element;
            try { base.InsertItem(index, item); }
            finally { _pendingAttachment = previousAttachment; }
        });
    }

    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        LayoutElement element = (LayoutElement)(ILayoutElement)this[index];
        Mutate(() =>
        {
            _source.Remove(element.DockModel);
            if (CountOccurrences(this[index]) > 1) _unprojectedChildren.Add(this[index]);
            base.RemoveItem(index);
            if (!Contains((T)(ILayoutElement)element)) _unprojectedChildren.Remove((T)(ILayoutElement)element);
        });
    }

    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();
        LayoutElement old = (LayoutElement)(ILayoutElement)this[index];
        // ObservableCollection still publishes a replacement for the same
        // instance. Its parent detach/attach hooks are part of that contract.
        LayoutElement replacement = ReferenceEquals(old, item) ? old : Prepare(item);
        Mutate(() =>
        {
            int nativeIndex = _source.IndexOf(old.DockModel);
            if (nativeIndex >= 0 && (ReferenceEquals(old, replacement) || !_source.Contains(replacement.DockModel)))
                _source[nativeIndex] = replacement.DockModel;
            else
            {
                _source.Remove(old.DockModel);
                if (!_source.Contains(replacement.DockModel)) _source.Insert(GetNativeInsertionIndex(index + 1), replacement.DockModel);
            }
            if (CountOccurrences(this[index]) > 1) _unprojectedChildren.Add(this[index]);
            _unprojectedChildren.Remove(item);
            LayoutElement? previousAttachment = _pendingAttachment;
            _pendingAttachment = replacement;
            try { base.SetItem(index, item); }
            finally { _pendingAttachment = previousAttachment; }
            if (!Contains((T)(ILayoutElement)old)) _unprojectedChildren.Remove((T)(ILayoutElement)old);
        });
    }

    private int CountOccurrences(T item)
    {
        int count = 0;
        foreach (T child in this) if (ReferenceEquals(child, item)) count++;
        return count;
    }

    private int GetNativeInsertionIndex(int index)
    {
        for (int i = index; i < Count; i++)
        {
            int nativeIndex = _source.IndexOf(((LayoutElement)(ILayoutElement)this[i]).DockModel);
            if (nativeIndex >= 0) return nativeIndex;
        }
        return _source.Count;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        using LayoutActiveContent.ValidationScope validation = !IsRefreshingSource &&
            (_owner is ILayoutContentSelector || e.Action == NotifyCollectionChangedAction.Replace && e.OldItems?[0] is LayoutContent) &&
            (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace) &&
            (_owner as LayoutRoot ?? _owner.Root as LayoutRoot) is { } root
                ? root.Focus.DeferValidation() : default;
        bool previousNotifying = IsNotifying;
        IsNotifying = true;
        try
        {
            base.OnCollectionChanged(e);
        }
        finally
        {
            IsNotifying = previousNotifying;
            if (!IsNotifying) NotificationCompleted?.Invoke();
        }
    }

    private void ApplyCollectionChange(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Parent and group hooks belong to one collection subscriber, as in
        // AvalonDock. The BCL reentrancy guard covers the entire hook sequence.
        if (!IsRefreshingSource && (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace) && e.OldItems is { } oldItems)
            foreach (object? item in oldItems)
                if (item is LayoutElement old && ReferenceEquals(old.Parent, _owner)) old.Parent = null;
        if (_pendingAttachment is { } attachment)
        {
            _pendingAttachment = null;
            Attach(attachment);
        }
        _notifyOwner?.Invoke();
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();
        if (newIndex < 0 || newIndex >= Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        Mutate(() =>
        {
            bool previous = IsMovingSource;
            IsMovingSource = true;
            try
            {
                base.MoveItem(oldIndex, newIndex);
                IDockable? selected = _model.ActiveDockable;
                MoveNativeItems(oldIndex, newIndex);
                // Native item generators can clear or change their selection
                // during a move. Keep the public collection policy's result;
                // MoveChild applies its protected hook after this projection.
                if (_owner is ILayoutContentSelector && !ReferenceEquals(_model.ActiveDockable, selected))
                {
                    if (_model.Factory is LayoutFactory factory) factory.SetPaneSelection(_model, selected);
                    else _model.ActiveDockable = selected;
                }
            }
            finally
            {
                IsMovingSource = previous;
                if (!previous && !IsNotifying) NotificationCompleted?.Invoke();
            }
        });
    }

    private void MoveNativeItems(int oldIndex, int newIndex)
    {
        if (MoveNativePane(oldIndex, newIndex)) return;
        // Groups also contain native splitters, normalized below. Repeated
        // facade occurrences must not create repeated native nodes.
        HashSet<IDockable> projected = new(_source);
        for (int i = _source.Count - 1; i >= 0; i--) _source.RemoveAt(i);
        foreach (T child in this)
        {
            IDockable model = ((LayoutElement)(ILayoutElement)child).DockModel;
            if (projected.Remove(model)) _source.Add(model);
        }
    }

    private bool MoveNativePane(int oldIndex, int newIndex)
    {
        if (_owner is not ILayoutContentSelector || _source.Count != Count) return false;
        if (_source is AvaloniaList<IDockable> avalonia) { avalonia.Move(oldIndex, newIndex); return true; }
        if (_source is ObservableCollection<IDockable> observable) { observable.Move(oldIndex, newIndex); return true; }
        return false;
    }

    protected override void ClearItems()
    {
        CheckReentrancy();
        Mutate(() =>
        {
            foreach (T child in this)
            {
                LayoutElement element = (LayoutElement)(ILayoutElement)child;
                element.RetainParentAfterCollectionReset();
                element.DockModel.Owner = null;
            }
            _source.Clear();
            _unprojectedChildren.Clear();
            base.ClearItems();
        });
    }

    private void Mutate(Action action)
    {
        _owner.VerifyAccess();
        bool previousSyncing = _syncing;
        _syncing = true;
        try
        {
            action();
            if (_visible)
            {
                if (_model is ProportionalDock) NormalizeSplitters();
                if (_owner is not ILayoutContentSelector && (_model.ActiveDockable is null || !_source.Contains(_model.ActiveDockable)))
                {
                    IDockable? selected = Count > 0 ? ((LayoutElement)(ILayoutElement)this[0]).DockModel : null;
                    // A public group has no selection/activation contract.
                    // Keep its native active child valid without replacing
                    // the focused document/tool with a structural container.
                    if (_model.Factory is LayoutFactory factory) factory.SetPaneSelection(_model, selected);
                    else _model.ActiveDockable = selected;
                }
                _model.DefaultDockable = _model.ActiveDockable;
                _model.IsEmpty = Count == 0;
            }
            else if (_model.Factory is { } factory)
            {
                foreach (IDockable child in _source) factory.InitDockable(child, _model);
            }
        }
        finally { _syncing = previousSyncing; }
    }

    private void NormalizeSplitters()
    {
        for (int i = _source.Count - 1; i >= 0; i--)
            if (_source[i] is ISplitter) _source.RemoveAt(i);
        for (int i = _source.Count - 1; i > 0; i--)
        {
            ProportionalDockSplitter splitter = new() { Owner = _model, Factory = _model.Factory, ResizePreview = true };
            _source.Insert(i, splitter);
        }
    }
}
