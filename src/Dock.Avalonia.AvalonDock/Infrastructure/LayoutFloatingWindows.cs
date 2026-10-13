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
    private LayoutFloatingWindow? _pendingAttachment;
    private IDockWindow? _publicMutationWindow;
    private HashSet<IDockWindow>? _replacementWindows;

    internal LayoutFloatingWindows(LayoutRoot owner)
    {
        _owner = owner;
        CollectionChanged += ApplyCollectionChange;
        Reconnect();
    }
    internal void Reconnect()
    {
        if (_source is INotifyCollectionChanged old) old.CollectionChanged -= Changed;
        RootDock root = (RootDock)_owner.DockModel;
        _source = root.Windows ??= new AvaloniaList<IDockWindow>();
        if (_source is INotifyCollectionChanged current) current.CollectionChanged += Changed;
        Refresh();
    }
    private void Changed(object? sender, NotifyCollectionChangedEventArgs args)
    {
        // A permitted nested public edit must finish its projection before
        // returning, even while another native collection change is being read.
        if (!_syncing || _publicMutationWindow is { } window && !IsBeingReplaced(window) &&
            (args.NewItems?.Contains(window) == true || args.OldItems?.Contains(window) == true)) Refresh();
    }
    private bool IsBeingReplaced(IDockWindow window) => _replacementWindows?.Contains(window) == true;
    private void Refresh()
    {
        bool previousSyncing = _syncing;
        _syncing = true;
        try
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_source.Contains(this[i].DockWindow)) continue;
                base.RemoveItem(i);
            }
            for (int i = 0; i < _source.Count; i++)
            {
                LayoutFloatingWindow window = _owner.Context.GetWindow(_source[i]);
                int oldIndex = IndexOf(window);
                if (oldIndex < 0) base.InsertItem(i, window);
                else
                {
                    if (!ReferenceEquals(window, _pendingAttachment)) window.Attach(_owner);
                    if (oldIndex != i) base.MoveItem(oldIndex, i);
                }
            }
        }
        finally { _syncing = previousSyncing; }
        _owner.NotifyStructureChanged();
    }
    protected override void InsertItem(int index, LayoutFloatingWindow item)
    {
        CheckReentrancy();
        ArgumentNullException.ThrowIfNull(item);
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        item.Parent?.RemoveChild(item);
        if (item.Parent is not null)
            throw new InvalidOperationException("The floating window's current host canceled removal from its layout.");
        item.PrepareAttachment(_owner);
        IDockWindow? previousMutation = _publicMutationWindow;
        _publicMutationWindow = item.DockWindow;
        try
        {
            using LayoutFloatingWindow.ParentProjectionScope parent = item.DeferParentProjection();
            _owner.DockFactory.InsertWindow((RootDock)_owner.DockModel, item.DockWindow, index);
            if (!Contains(item) && _source.Contains(item.DockWindow)) Refresh();
        }
        finally { _publicMutationWindow = previousMutation; }
        if (Contains(item)) _owner.Manager?.PresentFloatingWindow(item);
    }
    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        LayoutFloatingWindow item = this[index];
        IDockWindow? previousMutation = _publicMutationWindow;
        _publicMutationWindow = item.DockWindow;
        try
        {
            using LayoutFloatingWindow.ParentProjectionScope parent = item.DeferParentProjection();
            RemoveNativeWindow(item);
            if (!IsBeingReplaced(item.DockWindow) && Contains(item) && !_source.Contains(item.DockWindow)) Refresh();
        }
        finally { _publicMutationWindow = previousMutation; }
    }

    private void RemoveNativeWindow(LayoutFloatingWindow item)
    {
        if (item is LayoutAnchorableFloatingWindow { IsValid: false } &&
            item.DockWindow.Host is LayoutFloatingWindowControl { HasClosed: false } host)
        {
            host.RequestCloseFromLayout();
            // The public tool model detaches immediately. Native host and
            // factory ownership survive until the queued close completes.
            _source.Remove(item.DockWindow);
            return;
        }
        if (item.CloseHost())
        {
            // An insertion callback runs before the native factory assigns
            // ownership. Removing that pending registration cancels its init.
            if (item.DockWindow.Owner is null) _source.Remove(item.DockWindow);
            else _owner.DockFactory.RemoveWindow(item.DockWindow);
        }
    }
    protected override void SetItem(int index, LayoutFloatingWindow item)
    {
        CheckReentrancy();
        ArgumentNullException.ThrowIfNull(item);
        if (ReferenceEquals(this[index], item))
        {
            ReplacePublicItem(index, item);
            if (item.DockWindow.Owner is null && _source.Contains(item.DockWindow))
                _owner.DockFactory.InitDockWindow(item.DockWindow, _owner.DockModel);
            if (item.DockWindow.Host is IHostWindowCloseState { IsClosed: false } && !item.DockWindow.Host.IsTracked)
                _owner.Manager?.PresentFloatingWindow(item);
            _owner.NotifyStructureChanged();
            return;
        }
        if (Contains(item)) throw new InvalidOperationException("A floating window cannot occur twice in a layout.");
        LayoutFloatingWindow previous = this[index];
        bool previousSyncing = _syncing;
        IDockWindow? previousMutation = _publicMutationWindow;
        bool registeredReplacement = (_replacementWindows ??= new()).Add(previous.DockWindow);
        _publicMutationWindow = null;
        _syncing = true;
        try
        {
            using LayoutFloatingWindow.ParentProjectionScope outgoing = previous.DeferParentProjection();
            RemoveNativeWindow(previous);
            if (_source.Contains(previous.DockWindow))
            {
                _syncing = previousSyncing;
                Refresh();
                return;
            }
            item.Parent?.RemoveChild(item);
            if (item.Parent is not null)
                throw new InvalidOperationException("The floating window's current host canceled removal from its layout.");
            using LayoutFloatingWindow.ParentProjectionScope incoming = item.DeferParentProjection();
            item.PrepareAttachment(_owner);
            _owner.DockFactory.InsertWindow((RootDock)_owner.DockModel, item.DockWindow, Math.Min(index, _source.Count));
        }
        catch
        {
            _syncing = previousSyncing;
            Refresh();
            throw;
        }
        finally
        {
            _syncing = previousSyncing;
            _publicMutationWindow = previousMutation;
            if (registeredReplacement) _replacementWindows.Remove(previous.DockWindow);
        }
        ReplacePublicItem(index, item);
        _owner.Manager?.PresentFloatingWindow(item);
        Refresh();
    }
    private void ReplacePublicItem(int index, LayoutFloatingWindow item)
    {
        LayoutFloatingWindow? previous = _pendingAttachment;
        _pendingAttachment = item;
        try { base.SetItem(index, item); }
        finally { _pendingAttachment = previous; }
    }
    private void ApplyCollectionChange(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace && e.OldItems is { } previous)
            foreach (LayoutFloatingWindow window in previous)
                if (ReferenceEquals(window.Parent, _owner)) window.SetProjectedParent(null);
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace && e.NewItems is { } current)
            foreach (LayoutFloatingWindow window in current) window.Attach(_owner);
    }
    protected override void ClearItems()
    {
        CheckReentrancy();
        List<LayoutFloatingWindow> windows = new(this);
        for (int i = windows.Count - 1; i >= 0; i--) Remove(windows[i]);
    }
    protected override void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();
        if (newIndex < 0 || newIndex >= Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        bool previousSyncing = _syncing;
        IDockWindow? previousMutation = _publicMutationWindow;
        _publicMutationWindow = null;
        _syncing = true;
        try
        {
            IDockWindow window = _source[oldIndex];
            _source.RemoveAt(oldIndex);
            _source.Insert(newIndex, window);
            base.MoveItem(oldIndex, newIndex);
        }
        finally
        {
            _syncing = previousSyncing;
            _publicMutationWindow = previousMutation;
        }
        _owner.NotifyStructureChanged();
    }
}
