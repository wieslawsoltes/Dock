// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Dock.Controls.Flat;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockOrientation = Dock.Model.Core.Orientation;

namespace Dock.Avalonia.Internal;

internal sealed class DockFlatProportionalAdapter : IDisposable
{
    private readonly Dictionary<IDockable, DockFlatItemAdapter> _items = new(ReferenceEqualityComparer.Instance);

    public IFlatProportionalDock? GetDock(IProportionalDock? dock)
    {
        return dock is null ? null : (IFlatProportionalDock)GetItem(dock);
    }

    public void PruneUnreachable(IDockable? root)
    {
        if (root is null)
        {
            Dispose();
            return;
        }

        var reachable = new HashSet<IDockable>(ReferenceEqualityComparer.Instance);
        CollectReachable(root, reachable);

        foreach (var item in new List<KeyValuePair<IDockable, DockFlatItemAdapter>>(_items))
        {
            if (reachable.Contains(item.Key))
            {
                continue;
            }

            item.Value.Dispose();
            _items.Remove(item.Key);
        }
    }

    private DockFlatItemAdapter GetItem(IDockable dockable)
    {
        if (_items.TryGetValue(dockable, out var item))
        {
            return item;
        }

        item = dockable switch
        {
            IProportionalDock proportionalDock => new DockFlatDockAdapter(this, proportionalDock),
            IProportionalDockSplitter splitter => new DockFlatSplitterAdapter(splitter),
            _ => new DockFlatItemAdapter(dockable)
        };

        _items[dockable] = item;
        return item;
    }

    private static void CollectReachable(IDockable dockable, ISet<IDockable> reachable)
    {
        if (!reachable.Add(dockable)
            || dockable is not IDock { VisibleDockables: { } visibleDockables })
        {
            return;
        }

        foreach (var child in visibleDockables)
        {
            CollectReachable(child, reachable);
        }
    }

    public void Dispose()
    {
        foreach (var item in _items.Values)
        {
            item.Dispose();
        }

        _items.Clear();
    }

    private sealed class DockFlatItemList : IList<IFlatProportionalItem>, INotifyCollectionChanged, IDisposable
    {
        private readonly DockFlatProportionalAdapter _owner;
        private readonly IList<IDockable> _items;
        private readonly INotifyCollectionChanged? _collectionChanged;

        public DockFlatItemList(DockFlatProportionalAdapter owner, IList<IDockable> items)
        {
            _owner = owner;
            _items = items;
            _collectionChanged = items as INotifyCollectionChanged;

            if (_collectionChanged is not null)
            {
                _collectionChanged.CollectionChanged += OnCollectionChanged;
            }
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public int Count => _items.Count;

        public bool IsReadOnly => true;

        public IFlatProportionalItem this[int index]
        {
            get => _owner.GetItem(_items[index]);
            set => throw new NotSupportedException();
        }

        public int IndexOf(IFlatProportionalItem item)
        {
            if (item is not DockFlatItemAdapter adapter)
            {
                return -1;
            }

            return _items.IndexOf(adapter.Dockable);
        }

        public bool Contains(IFlatProportionalItem item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(IFlatProportionalItem[] array, int arrayIndex)
        {
            for (var index = 0; index < _items.Count; index++)
            {
                array[arrayIndex + index] = _owner.GetItem(_items[index]);
            }
        }

        public IEnumerator<IFlatProportionalItem> GetEnumerator()
        {
            foreach (var dockable in _items)
            {
                yield return _owner.GetItem(dockable);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Insert(int index, IFlatProportionalItem item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(IFlatProportionalItem item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(IFlatProportionalItem item)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            if (_collectionChanged is not null)
            {
                _collectionChanged.CollectionChanged -= OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, Translate(e));
        }

        private NotifyCollectionChangedEventArgs Translate(NotifyCollectionChangedEventArgs e)
        {
            return e.Action switch
            {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    Wrap(e.NewItems),
                    e.NewStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    Wrap(e.OldItems),
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    Wrap(e.NewItems),
                    Wrap(e.OldItems),
                    e.NewStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Move,
                    Wrap(e.NewItems),
                    e.NewStartingIndex,
                    e.OldStartingIndex),
                _ => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            };
        }

        private IList Wrap(IList? items)
        {
            var result = new List<IFlatProportionalItem>();
            if (items is null)
            {
                return result;
            }

            foreach (var item in items)
            {
                if (item is IDockable dockable)
                {
                    result.Add(_owner.GetItem(dockable));
                }
            }

            return result;
        }
    }

    internal class DockFlatItemAdapter : IFlatProportionalItem, INotifyPropertyChanged, IDisposable
    {
        private readonly INotifyPropertyChanged? _dockablePropertyChanged;

        public DockFlatItemAdapter(IDockable dockable)
        {
            Dockable = dockable;
            _dockablePropertyChanged = dockable as INotifyPropertyChanged;

            if (_dockablePropertyChanged is not null)
            {
                _dockablePropertyChanged.PropertyChanged += OnDockablePropertyChanged;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDockable Dockable { get; }

        public object Key => Dockable;

        public object? Content => Dockable;

        public double Proportion
        {
            get => Dockable.Proportion;
            set => Dockable.Proportion = value;
        }

        public double CollapsedProportion
        {
            get => Dockable.CollapsedProportion;
            set => Dockable.CollapsedProportion = value;
        }

        public double MinWidth => Dockable.MinWidth;

        public double MinHeight => Dockable.MinHeight;

        public double MaxWidth => Dockable.MaxWidth;

        public double MaxHeight => Dockable.MaxHeight;

        public bool IsCollapsable => Dockable.IsCollapsable;

        public bool IsEmpty => Dockable.IsEmpty;

        protected virtual string? MapPropertyName(string? propertyName)
        {
            return propertyName;
        }

        private void OnDockablePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(MapPropertyName(e.PropertyName)));
        }

        public virtual void Dispose()
        {
            if (_dockablePropertyChanged is not null)
            {
                _dockablePropertyChanged.PropertyChanged -= OnDockablePropertyChanged;
            }
        }
    }

    internal sealed class DockFlatDockAdapter : DockFlatItemAdapter, IFlatProportionalDock
    {
        private readonly DockFlatProportionalAdapter _owner;
        private IList<IDockable>? _sourceVisibleItems;
        private DockFlatItemList? _visibleItems;

        public DockFlatDockAdapter(DockFlatProportionalAdapter owner, IProportionalDock dock)
            : base(dock)
        {
            _owner = owner;
            Dock = dock;
        }

        public IProportionalDock Dock { get; }

        public global::Avalonia.Layout.Orientation Orientation => Dock.Orientation == DockOrientation.Vertical
            ? global::Avalonia.Layout.Orientation.Vertical
            : global::Avalonia.Layout.Orientation.Horizontal;

        public IList<IFlatProportionalItem>? VisibleItems
        {
            get
            {
                if (Dock.VisibleDockables is null)
                {
                    _sourceVisibleItems = null;
                    _visibleItems?.Dispose();
                    _visibleItems = null;
                    return null;
                }

                if (ReferenceEquals(_sourceVisibleItems, Dock.VisibleDockables) && _visibleItems is not null)
                {
                    return _visibleItems;
                }

                _visibleItems?.Dispose();
                _sourceVisibleItems = Dock.VisibleDockables;
                _visibleItems = new DockFlatItemList(_owner, Dock.VisibleDockables);
                return _visibleItems;
            }
        }

        protected override string? MapPropertyName(string? propertyName)
        {
            return propertyName switch
            {
                nameof(IDock.VisibleDockables) => nameof(VisibleItems),
                nameof(IProportionalDock.Orientation) => nameof(Orientation),
                _ => propertyName
            };
        }

        public override void Dispose()
        {
            _visibleItems?.Dispose();
            _visibleItems = null;
            _sourceVisibleItems = null;
            base.Dispose();
        }
    }

    internal sealed class DockFlatSplitterAdapter : DockFlatItemAdapter, IFlatProportionalSplitter
    {
        public DockFlatSplitterAdapter(IProportionalDockSplitter splitter)
            : base(splitter)
        {
            Splitter = splitter;
        }

        public IProportionalDockSplitter Splitter { get; }

        public bool CanResize => Splitter.CanResize;

        public bool ResizePreview => Splitter.ResizePreview;
    }
}
