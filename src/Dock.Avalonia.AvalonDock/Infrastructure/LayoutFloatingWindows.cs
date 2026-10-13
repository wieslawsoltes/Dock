using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutFloatingWindows : ObservableCollection<LayoutFloatingWindow>
{
    private readonly LayoutRoot _owner;
    private IList<IDockWindow> _source = null!;
    private bool _syncing;

    internal LayoutFloatingWindows(LayoutRoot owner) { _owner = owner; Reconnect(); }
    internal void Reconnect()
    {
        if (_source is INotifyCollectionChanged old) old.CollectionChanged -= Changed;
        RootDock root = (RootDock)_owner.DockModel;
        _source = root.Windows ??= new AvaloniaList<IDockWindow>();
        if (_source is INotifyCollectionChanged current) current.CollectionChanged += Changed;
        Refresh();
    }
    private void Changed(object? sender, NotifyCollectionChangedEventArgs args) { if (!_syncing) Refresh(); }
    private void Refresh()
    {
        _syncing = true;
        try
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_source.Contains(this[i].DockWindow)) continue;
                this[i].SetProjectedParent(null);
                base.RemoveItem(i);
            }
            for (int i = 0; i < _source.Count; i++)
            {
                LayoutFloatingWindow window = _owner.Context.GetWindow(_source[i]);
                window.Attach(_owner);
                int oldIndex = IndexOf(window);
                if (oldIndex < 0) base.InsertItem(i, window);
                else if (oldIndex != i) base.MoveItem(oldIndex, i);
            }
        }
        finally { _syncing = false; }
        _owner.NotifyStructureChanged();
    }
    protected override void InsertItem(int index, LayoutFloatingWindow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        item.Parent?.RemoveChild(item);
        if (item.Parent is not null)
            throw new InvalidOperationException("The floating window's current host canceled removal from its layout.");
        item.Attach(_owner);
        _owner.DockFactory.InsertWindow((RootDock)_owner.DockModel, item.DockWindow, index);
        _owner.Manager?.PresentFloatingWindow(item);
    }
    protected override void RemoveItem(int index)
    {
        LayoutFloatingWindow item = this[index];
        if (item is LayoutAnchorableFloatingWindow { IsValid: false } &&
            item.DockWindow.Host is LayoutFloatingWindowControl { HasClosed: false } host)
        {
            host.RequestCloseFromLayout();
            // The public tool model detaches immediately. Native host and
            // factory ownership survive until the queued close completes.
            _source.Remove(item.DockWindow);
            return;
        }
        if (item.CloseHost()) _owner.DockFactory.RemoveWindow(item.DockWindow);
    }
    protected override void SetItem(int index, LayoutFloatingWindow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (ReferenceEquals(this[index], item))
        {
            base.SetItem(index, item);
            _owner.NotifyStructureChanged();
            return;
        }
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        LayoutFloatingWindow previous = this[index];
        RemoveItem(index);
        if (!Contains(previous)) InsertItem(index, item);
    }
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Replace)
        {
            if (e.OldItems is { } previous)
                foreach (LayoutFloatingWindow window in previous)
                    if (ReferenceEquals(window.Parent, _owner)) window.SetProjectedParent(null);
            if (e.NewItems is { } current)
                foreach (LayoutFloatingWindow window in current) window.Attach(_owner);
        }
        base.OnCollectionChanged(e);
    }
    protected override void ClearItems()
    {
        List<LayoutFloatingWindow> windows = new(this);
        for (int i = windows.Count - 1; i >= 0; i--) Remove(windows[i]);
    }
    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (newIndex < 0 || newIndex >= Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        _syncing = true;
        try
        {
            IDockWindow window = _source[oldIndex];
            _source.RemoveAt(oldIndex);
            _source.Insert(newIndex, window);
            base.MoveItem(oldIndex, newIndex);
        }
        finally { _syncing = false; }
        _owner.NotifyStructureChanged();
    }
}
