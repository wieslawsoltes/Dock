using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

internal sealed class LayoutChildren<T> : ObservableCollection<T> where T : class, ILayoutElement
{
    private readonly LayoutElement _owner;
    private readonly DockBase _model;
    private readonly Func<IList<IDockable>?> _getSource;
    private readonly Action<IList<IDockable>> _setSource;
    private readonly bool _visible;
    private IList<IDockable> _source = null!;
    private bool _syncing;

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
        if (!_syncing) Refresh();
    }

    private void Refresh()
    {
        _syncing = true;
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
        finally { _syncing = false; }
    }

    private LayoutElement Prepare(T item)
    {
        if (item is not LayoutElement element) throw new ArgumentException("Children must use Dock-backed layout elements.", nameof(item));
        for (ILayoutElement? ancestor = _owner; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, item)) throw new InvalidOperationException("A layout cannot contain itself or an ancestor.");
        if (Contains(item)) throw new InvalidOperationException("A layout element cannot occur twice in one group.");
        if (_owner is LayoutAnchorGroup && element is LayoutContent content) content.RememberDockedLocation();
        element.Parent?.RemoveChild(element);
        _owner.Context.Adopt(element.Context);
        if (_model.Factory is { } factory) factory.InitDockable(element.DockModel, _model);
        else element.DockModel.Owner = _model;
        return element;
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
            base.InsertItem(index, item);
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
            base.SetItem(index, item);
            old.DockModel.Owner = null;
        });
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (newIndex < 0 || newIndex >= Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        Mutate(() =>
        {
            base.MoveItem(oldIndex, newIndex);
            // Reorder the actual models, preserving the selected item and wrappers.
            for (int i = _source.Count - 1; i >= 0; i--) _source.RemoveAt(i);
            foreach (T child in this) _source.Add(((LayoutElement)(ILayoutElement)child).DockModel);
        });
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
        _syncing = true;
        try
        {
            action();
            if (_visible)
            {
                if (_model is ProportionalDock) NormalizeSplitters();
                if (_model.ActiveDockable is null || !_source.Contains(_model.ActiveDockable))
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
