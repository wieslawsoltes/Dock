using System;
using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Synchronizes a presentation with the existing pane; publishers keep only a
// weak reference so an application can retain a model without retaining its UI.
internal sealed class LayoutPaneControlBinding(LayoutCachePaneControl control, LayoutElement model,
    ILayoutContentSelector selection, IEnumerable children, bool documents)
{
    private Subscription? _subscription;
    private bool _refreshing;
    private bool _selectionPending;
    private bool _selectionQueued;
    private bool IsUpdatingChildren => children is ILayoutChildrenNotification { IsNotifying: true }
        || selection is ILayoutPaneSelection pane && (pane.IsRefreshingChildren || pane.IsMovingChildren);

    internal void Initialize()
    {
        _refreshing = true;
        try { control.ItemsSource = children; }
        finally { _refreshing = false; }
        _subscription = new(model, children as ILayoutChildrenNotification, this);
        RefreshSelection();
        RefreshManager();
    }

    private void RefreshSelection()
    {
        if (_refreshing || IsUpdatingChildren)
        {
            _selectionPending = true;
            if (!_refreshing && !_selectionQueued)
            {
                _selectionQueued = true;
                WeakReference<LayoutPaneControlBinding> reference = new(this);
                Dispatcher.UIThread.Post(() =>
                {
                    if (!reference.TryGetTarget(out LayoutPaneControlBinding? binding)) return;
                    binding._selectionQueued = false;
                    binding.RefreshSelection();
                });
            }
            return;
        }
        _refreshing = true;
        try
        {
            do
            {
                _selectionPending = false;
                control.SetCurrentValue(SelectingItemsControl.SelectedItemProperty, selection.SelectedContent);
            } while (_selectionPending);
        }
        finally { _refreshing = false; }
    }

    internal void SelectFromControl(SelectionChangedEventArgs args, bool activate)
    {
        if (_refreshing || IsUpdatingChildren) return;
        // Collection reconciliation belongs to the public pane model. Avalonia
        // may clear its selection before that model finishes a native removal.
        if (control.SelectedItem is not LayoutContent content)
        {
            foreach (object? removed in args.RemovedItems)
                if (removed is LayoutContent previous && ReferenceEquals(selection.SelectedContent, previous) && selection.IndexOf(previous) >= 0)
                {
                    selection.SelectedContentIndex = -1;
                    break;
                }
            return;
        }
        int index = selection.IndexOf(content);
        if (index < 0) return;
        selection.SelectedContentIndex = index;
        if (activate && ReferenceEquals(selection.SelectedContent, content)) content.IsActive = true;
    }

    internal void ActivateSelectedContent()
    {
        if (selection.SelectedContent is { } content) content.IsActive = true;
    }

    internal void ActivateFromPointer(object? source, bool handled)
    {
        // Pointer hooks belong to the manager containing the source visual.
        // Keyboard focus has a separate, selection-only contract.
        if (!handled && LayoutInputOwnership.IsWithinManager(source, model.Root?.Manager))
            ActivateSelectedContent();
    }

    private void RefreshManager()
    {
        DockingManager? manager = model.Root?.Manager;
        _subscription?.WatchManager(manager);
        control.SetCurrentValue(StyledElement.ThemeProperty,
            documents ? manager?.DocumentPaneControlStyle : manager?.AnchorablePaneControlStyle);
        if (manager is not null) control.SetCurrentValue(Visual.FlowDirectionProperty, manager.FlowDirection);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly WeakReference<LayoutElement> _model;
        private readonly WeakReference<LayoutPaneControlBinding> _owner;
        private readonly WeakReference<ILayoutChildrenNotification>? _children;
        private WeakReference<DockingManager>? _manager;

        internal Subscription(LayoutElement model, ILayoutChildrenNotification? children, LayoutPaneControlBinding owner)
        {
            _model = new(model);
            _owner = new(owner);
            _children = children is null ? null : new(children);
            model.PropertyChanged += ModelChanged;
            if (children is not null) children.NotificationCompleted += ChildrenChanged;
        }

        private void ChildrenChanged()
        {
            if (_owner.TryGetTarget(out LayoutPaneControlBinding? owner)) owner.RefreshSelection();
            else Dispose();
        }

        internal void WatchManager(DockingManager? manager)
        {
            DockingManager? previous = _manager?.TryGetTarget(out DockingManager? target) == true ? target : null;
            if (ReferenceEquals(previous, manager)) return;
            if (previous is not null) previous.PropertyChanged -= ManagerChanged;
            _manager = manager is null ? null : new(manager);
            if (manager is not null) manager.PropertyChanged += ManagerChanged;
        }

        private void ModelChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutPaneControlBinding? owner)) { Dispose(); return; }
            if (args.PropertyName is nameof(ILayoutContentSelector.SelectedContent) or nameof(ILayoutContentSelector.SelectedContentIndex))
                owner.RefreshSelection();
            if (args.PropertyName is nameof(ILayoutElement.Root) or nameof(ILayoutElement.Parent)) owner.RefreshManager();
        }

        private void ManagerChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutPaneControlBinding? owner)) { Dispose(); return; }
            if (args.Property == DockingManager.DocumentPaneControlStyleProperty ||
                args.Property == DockingManager.AnchorablePaneControlStyleProperty || args.Property == Visual.FlowDirectionProperty)
                owner.RefreshManager();
        }

        public void Dispose()
        {
            if (_model.TryGetTarget(out LayoutElement? model)) model.PropertyChanged -= ModelChanged;
            if (_children?.TryGetTarget(out ILayoutChildrenNotification? children) == true) children.NotificationCompleted -= ChildrenChanged;
            WatchManager(null);
        }
    }
}
