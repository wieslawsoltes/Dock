using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Core;

namespace AvalonDock.Infrastructure;

// Observe native host identities independently of floating model membership.
// A model can outlive a closed host and receive a new host on reattachment.
internal sealed class LayoutFloatingWindowControls
{
    private readonly DockingManager _manager;
    private readonly ObservableCollection<LayoutFloatingWindowControl> _items = new();
    private readonly List<DockWindow> _windows = new();
    private LayoutRoot? _layout;
    private bool _refreshing;
    private bool _pending;

    internal LayoutFloatingWindowControls(DockingManager manager)
    {
        _manager = manager;
        Items = new ReadOnlyObservableCollection<LayoutFloatingWindowControl>(_items);
    }

    internal ReadOnlyObservableCollection<LayoutFloatingWindowControl> Items { get; }

    internal void SetLayout(LayoutRoot layout)
    {
        if (ReferenceEquals(_layout, layout)) return;
        if (_layout is not null) _layout.FloatingWindows.CollectionChanged -= ModelsChanged;
        foreach (DockWindow window in _windows) window.PropertyChanged -= HostChanged;
        _windows.Clear();
        _layout = layout;
        layout.FloatingWindows.CollectionChanged += ModelsChanged;
        Refresh();
    }

    internal void Remove(LayoutFloatingWindowControl host)
    {
        int index = _items.IndexOf(host);
        if (index < 0) return;
        _items.RemoveAt(index);
        _manager.NotifyFloatingWindowControlsChanged(new(NotifyCollectionChangedAction.Remove, host, index));
    }

    private void ModelsChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
    private void HostChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == DockWindow.HostProperty) Refresh();
    }

    private void Refresh()
    {
        if (_refreshing) { _pending = true; return; }
        _refreshing = true;
        try
        {
            do
            {
                _pending = false;
                Synchronize();
            } while (_pending);
        }
        finally { _refreshing = false; }
    }

    private void Synchronize()
    {
        if (_layout is null) return;
        List<DockWindow> current = new(_layout.FloatingWindows.Count);
        List<LayoutFloatingWindowControl> hosts = new(current.Capacity);
        foreach (LayoutFloatingWindow model in _layout.FloatingWindows)
        {
            current.Add(model.DockWindow);
            if (model.DockWindow.Host is LayoutFloatingWindowControl { HasClosed: false } host) hosts.Add(host);
        }
        for (int i = _windows.Count - 1; i >= 0; i--)
        {
            if (current.Contains(_windows[i])) continue;
            _windows[i].PropertyChanged -= HostChanged;
            _windows.RemoveAt(i);
        }
        foreach (DockWindow window in current)
        {
            if (_windows.Contains(window)) continue;
            _windows.Add(window);
            window.PropertyChanged += HostChanged;
        }
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            if (hosts.Contains(_items[i])) continue;
            Remove(_items[i]);
            if (_pending) return;
        }
        foreach (LayoutFloatingWindowControl host in hosts)
        {
            if (_items.Contains(host)) continue;
            int index = _items.Count;
            _items.Add(host);
            _manager.NotifyFloatingWindowControlsChanged(new(NotifyCollectionChangedAction.Add, host, index));
            if (_pending) return;
        }
    }
}
