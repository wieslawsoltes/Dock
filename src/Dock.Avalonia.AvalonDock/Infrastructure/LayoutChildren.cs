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
    private IList<IDockable> _source = null!;
    private bool _syncing;
    private LayoutElement? _pendingAttachment;
    internal bool IsRefreshingSource { get; private set; }
    public bool IsNotifying { get; private set; }
    public event Action? NotificationCompleted;

    internal LayoutChildren(LayoutElement owner, DockBase model,
        Func<IList<IDockable>?>? getSource = null, Action<IList<IDockable>>? setSource = null)
    {
        _owner = owner;
        _model = model;
        _visible = getSource is null;
        _getSource = getSource ?? (() => model.VisibleDockables);
        _setSource = setSource ?? (value => model.VisibleDockables = value);
        Reconnect();
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
            int target = 0;
            foreach (IDockable model in _source)
            {
                if (model is ISplitter) continue;
                T child = (T)(ILayoutElement)_owner.Context.Get(model);
                if (target < Count && ReferenceEquals(this[target], child)) { target++; continue; }
                int existing = IndexOf(child);
                if (existing >= 0) base.MoveItem(existing, target);
                else base.InsertItem(target, child);
                target++;
            }
            while (Count > target) base.RemoveItem(Count - 1);
        }
        finally { IsRefreshingSource = false; _syncing = false; }
    }

    private LayoutElement Prepare(T item)
    {
        _owner.VerifyAccess();
        if (item is not LayoutElement element) throw new ArgumentException("Children must use Dock-backed layout elements.", nameof(item));
        for (ILayoutElement? ancestor = _owner; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, item)) throw new InvalidOperationException("A layout cannot contain itself or an ancestor.");
        if (Contains(item)) throw new InvalidOperationException("A layout element cannot occur twice in one group.");
        if (_owner is LayoutAnchorGroup && element is LayoutContent content) content.RememberDockedLocation();
        element.Parent?.RemoveChild(element);
        _owner.Context.Adopt(element.Context);
        return element;
    }

    private void Attach(LayoutElement element)
    {
        if (_model.Factory is { } factory) factory.InitDockable(element.DockModel, _model);
        else element.DockModel.Owner = _model;
    }

    protected override void InsertItem(int index, T item)
    {
        if (_syncing) { base.InsertItem(index, item); return; }
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
        LayoutElement element = Prepare(item);
        Mutate(() =>
        {
            int nativeIndex = index < Count ? _source.IndexOf(((LayoutElement)(ILayoutElement)this[index]).DockModel) : _source.Count;
            _source.Insert(nativeIndex, element.DockModel);
            _pendingAttachment = element;
            try { base.InsertItem(index, item); }
            finally { _pendingAttachment = null; }
        });
    }

    protected override void RemoveItem(int index)
    {
        LayoutElement element = (LayoutElement)(ILayoutElement)this[index];
        Mutate(() =>
        {
            _source.Remove(element.DockModel);
            base.RemoveItem(index);
            if (ReferenceEquals(element.DockModel.Owner, _model)) element.DockModel.Owner = null;
        });
    }

    protected override void SetItem(int index, T item)
    {
        if (ReferenceEquals(this[index], item)) return;
        LayoutElement old = (LayoutElement)(ILayoutElement)this[index];
        LayoutElement replacement = Prepare(item);
        Mutate(() =>
        {
            _source[_source.IndexOf(old.DockModel)] = replacement.DockModel;
            _pendingAttachment = replacement;
            try { base.SetItem(index, item); }
            finally { _pendingAttachment = null; }
        });
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        bool previousNotifying = IsNotifying;
        IsNotifying = true;
        try
        {
            // ObservableCollection has inserted the item at this point, so parent
            // hooks can resolve its index. Publish only after native initialization.
            if (_pendingAttachment is { } attachment)
            {
                _pendingAttachment = null;
                if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems?[0] is LayoutElement previous &&
                    ReferenceEquals(previous.DockModel.Owner, _model)) previous.DockModel.Owner = null;
                Attach(attachment);
            }
            base.OnCollectionChanged(e);
        }
        finally
        {
            IsNotifying = previousNotifying;
            if (!IsNotifying) NotificationCompleted?.Invoke();
        }
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (newIndex < 0 || newIndex >= Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        Mutate(() =>
        {
            base.MoveItem(oldIndex, newIndex);
            ILayoutContentSelector? selector = _owner as ILayoutContentSelector;
            LayoutContent? selected = selector?.SelectedContent;
            if (!MoveNativePane(oldIndex, newIndex))
            {
                // Groups also contain native splitters, normalized below.
                for (int i = _source.Count - 1; i >= 0; i--) _source.RemoveAt(i);
                foreach (T child in this) _source.Add(((LayoutElement)(ILayoutElement)child).DockModel);
            }
            // Native item generators can transiently deselect a moved container.
            // Restore through the passive pane API without changing activation.
            if (selected is not null && selector is { SelectedContent: null })
            {
                int index = selector.IndexOf(selected);
                if (index >= 0) selector.SelectedContentIndex = index;
            }
        });
    }

    private bool MoveNativePane(int oldIndex, int newIndex)
    {
        if (_owner is not ILayoutContentSelector) return false;
        if (_source is AvaloniaList<IDockable> avalonia) { avalonia.Move(oldIndex, newIndex); return true; }
        if (_source is ObservableCollection<IDockable> observable) { observable.Move(oldIndex, newIndex); return true; }
        return false;
    }

    protected override void ClearItems()
    {
        Mutate(() =>
        {
            foreach (T child in this) ((LayoutElement)(ILayoutElement)child).DockModel.Owner = null;
            _source.Clear();
            base.ClearItems();
        });
    }

    private void Mutate(Action action)
    {
        _owner.VerifyAccess();
        _syncing = true;
        try
        {
            action();
            if (_visible)
            {
                if (_model is ProportionalDock) NormalizeSplitters();
                if (_owner is not ILayoutContentSelector && (_model.ActiveDockable is null || !_source.Contains(_model.ActiveDockable)))
                    _model.ActiveDockable = Count > 0 ? ((LayoutElement)(ILayoutElement)this[0]).DockModel : null;
                _model.DefaultDockable = _model.ActiveDockable;
                _model.IsEmpty = Count == 0;
            }
            else if (_model.Factory is { } factory)
            {
                foreach (IDockable child in _source) factory.InitDockable(child, _model);
            }
        }
        finally { _syncing = false; }
    }

    private void NormalizeSplitters()
    {
        for (int i = _source.Count - 1; i >= 0; i--)
            if (_source[i] is ISplitter) _source.RemoveAt(i);
        for (int i = _source.Count - 1; i > 0; i--)
        {
            ProportionalDockSplitter splitter = new() { Owner = _model, Factory = _model.Factory };
            _source.Insert(i, splitter);
        }
    }
}
