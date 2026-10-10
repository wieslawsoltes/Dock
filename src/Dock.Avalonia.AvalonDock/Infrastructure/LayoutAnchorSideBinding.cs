using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

internal sealed class LayoutAnchorSideBinding : IDisposable
{
    private readonly LayoutRoot _root;
    private readonly LayoutAnchorSide _side;
    private readonly Dictionary<LayoutAnchorable, LayoutAnchorGroup> _projected = new();
    private IList<IDockable> _pinned = null!;
    private bool _busy;

    internal LayoutAnchorSideBinding(LayoutRoot root, LayoutAnchorSide side)
    {
        _root = root;
        _side = side;
        ReconnectCore();
        _side.ChildrenTreeChanged += TreeChanged;
        if (side.IsVisible) Push();
        else Pull();
    }

    internal void Reconnect()
    {
        ReconnectCore();
        Pull();
    }

    private void ReconnectCore()
    {
        if (_pinned is INotifyCollectionChanged old) old.CollectionChanged -= PinnedChanged;
        RootDock root = (RootDock)_root.DockModel;
        _pinned = _side.Side switch
        {
            AnchorSide.Left => root.LeftPinnedDockables ??= new AvaloniaList<IDockable>(),
            AnchorSide.Top => root.TopPinnedDockables ??= new AvaloniaList<IDockable>(),
            AnchorSide.Right => root.RightPinnedDockables ??= new AvaloniaList<IDockable>(),
            _ => root.BottomPinnedDockables ??= new AvaloniaList<IDockable>()
        };
        if (_pinned is INotifyCollectionChanged current) current.CollectionChanged += PinnedChanged;
    }

    private void TreeChanged(object? sender, ChildrenTreeChangedEventArgs args)
    {
        if (!_busy) Push();
    }

    private void PinnedChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!_busy) Pull();
    }

    private void Push()
    {
        _busy = true;
        try
        {
            Dictionary<LayoutAnchorable, LayoutAnchorGroup> desired = new();
            List<IDockable> order = new();
            foreach (LayoutAnchorGroup group in _side.Children)
            {
                foreach (LayoutAnchorable tool in group.Children)
                {
                    desired.Add(tool, group);
                    order.Add(tool.DockModel);
                    tool.SetProjectedParent(group);
                    group.PreviousContainer ??= tool.PreviousContainer;
                }
            }
            foreach (var pair in _projected)
            {
                if (desired.ContainsKey(pair.Key)) continue;
                pair.Key.SetProjectedParent(null);
                _pinned.Remove(pair.Key.DockModel);
                // Public collection removal detaches the tool. Native operations
                // use Pull and keep their own ownership transitions intact.
                _root.DockFactory.InitDockable(pair.Key.DockModel, null);
            }
            for (int i = _pinned.Count - 1; i >= 0; i--)
                if (!order.Contains(_pinned[i])) _pinned.RemoveAt(i);
            for (int i = 0; i < order.Count; i++)
            {
                IDockable model = order[i];
                int oldIndex = _pinned.IndexOf(model);
                if (oldIndex == i) continue;
                if (oldIndex >= 0) _pinned.RemoveAt(oldIndex);
                _pinned.Insert(i, model);
            }
            _projected.Clear();
            foreach (var pair in desired)
            {
                _projected.Add(pair.Key, pair.Value);
                _root.DockFactory.InitDockable(pair.Key.DockModel, _root.DockModel);
            }
        }
        finally { _busy = false; }
    }

    private void Pull()
    {
        _busy = true;
        try
        {
            List<LayoutAnchorable> removed = new();
            HashSet<LayoutAnchorGroup> changedGroups = new();
            foreach (var pair in _projected)
            {
                if (_pinned.Contains(pair.Key.DockModel)) continue;
                pair.Value.RemoveProjectedChild(pair.Key);
                changedGroups.Add(pair.Value);
                pair.Key.SetProjectedParent(null);
                removed.Add(pair.Key);
            }
            foreach (LayoutAnchorable tool in removed) _projected.Remove(tool);
            foreach (IDockable model in _pinned)
            {
                LayoutAnchorable tool = (LayoutAnchorable)_root.Context.Get(model);
                if (_projected.ContainsKey(tool)) continue;
                tool.RememberDockedLocation();
                LayoutAnchorGroup? group = null;
                foreach (LayoutAnchorGroup candidate in _side.Children)
                    if (ReferenceEquals(candidate.PreviousContainer, tool.PreviousContainer)) { group = candidate; break; }
                if (group is null)
                {
                    group = new LayoutAnchorGroup { PreviousContainer = tool.PreviousContainer };
                    _side.Children.Add(group);
                }
                tool.SetProjectedParent(group);
                group.AddProjectedChild(tool);
                _projected.Add(tool, group);
            }
            foreach (LayoutAnchorGroup group in changedGroups)
                if (group.ChildrenCount == 0 && !IsReturnDestination(group)) _side.Children.Remove(group);
            List<LayoutAnchorGroup> orderedGroups = new();
            Dictionary<LayoutAnchorGroup, int> positions = new();
            foreach (IDockable model in _pinned)
            {
                LayoutAnchorable tool = (LayoutAnchorable)_root.Context.Get(model);
                LayoutAnchorGroup group = _projected[tool];
                if (!positions.TryGetValue(group, out int position)) orderedGroups.Add(group);
                int oldIndex = group.Children.IndexOf(tool);
                if (oldIndex != position) group.Children.Move(oldIndex, position);
                positions[group] = position + 1;
            }
            for (int i = 0; i < orderedGroups.Count; i++)
            {
                int oldIndex = _side.Children.IndexOf(orderedGroups[i]);
                if (oldIndex != i) _side.Children.Move(oldIndex, i);
            }
        }
        finally { _busy = false; }
    }

    private bool IsReturnDestination(LayoutAnchorGroup group)
    {
        foreach (LayoutAnchorable hidden in _root.Hidden)
            if (ReferenceEquals(hidden.PreviousContainer, group) || ReferenceEquals(hidden.InitialContainer, group)) return true;
        return false;
    }

    public void Dispose()
    {
        _side.ChildrenTreeChanged -= TreeChanged;
        if (_pinned is INotifyCollectionChanged changes) changes.CollectionChanged -= PinnedChanged;
        foreach (var pair in _projected)
        {
            _pinned.Remove(pair.Key.DockModel);
            pair.Key.SetProjectedParent(null);
            pair.Key.DockModel.Owner = pair.Value.DockModel;
            pair.Key.DockModel.Factory = null;
            pair.Key.DockModel.DockingState = DockingWindowState.None;
        }
        _projected.Clear();
    }
}
