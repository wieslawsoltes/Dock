using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

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
        _owner.NotifyUpdated();
    }
    protected override void InsertItem(int index, LayoutFloatingWindow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        item.Parent?.RemoveChild(item);
        item.Attach(_owner);
        _owner.DockFactory.InsertWindow((RootDock)_owner.DockModel, item.DockWindow, index);
        _owner.Manager?.PresentFloatingWindow(item);
    }
    protected override void RemoveItem(int index)
    {
        LayoutFloatingWindow item = this[index];
        item.CloseHost();
        _owner.DockFactory.RemoveWindow(item.DockWindow);
    }
    protected override void SetItem(int index, LayoutFloatingWindow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (ReferenceEquals(this[index], item)) return;
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        RemoveItem(index);
        InsertItem(index, item);
    }
    protected override void ClearItems()
    {
        for (int i = Count - 1; i >= 0; i--) RemoveItem(i);
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
        _owner.NotifyUpdated();
    }
}
