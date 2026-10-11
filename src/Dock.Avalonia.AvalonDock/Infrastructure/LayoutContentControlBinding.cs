using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Public content controls compose the existing item/view without owning its
// lifetime. Model and container publishers must not retain discarded controls.
internal sealed class LayoutContentControlBinding(TemplatedControl control,
    Action<LayoutItem?> setItem, Action clearModel)
{
    private LayoutContent? _model;
    private Subscription? _subscription;
    internal bool IsNativePresentation { get; set; }

    internal void SetModel(LayoutContent? model)
    {
        _subscription?.Dispose();
        _model = model;
        _subscription = model is null ? null : new Subscription(model, this);
        control.SetCurrentValue(InputElement.IsEnabledProperty, model?.IsEnabled ?? true);
        RefreshItem();
    }

    internal void RefreshItem()
    {
        LayoutItem? item = _model?.DockModel.Factory is LayoutFactory factory
            ? factory.GetLayoutItem(_model.DockModel)
            : _model?.Root?.Manager?.GetLayoutItemFromModel(_model);
        setItem(item);
    }

    internal void WatchItem(LayoutItem? item) => _subscription?.SetItem(item);

    internal void ActivateFromFocus(FocusChangedEventArgs args)
    {
        if (args.NewFocusedElement is not null && args.OldFocusedElement is LayoutFloatingWindowControl) return;
        // The native deferred presenter can still show the previous selection
        // while a modal owner restores focus. Do not undo the new selection.
        if (IsNativePresentation && _model?.IsSelected != true && args.NavigationMethod == NavigationMethod.Unspecified) return;
        Activate(args.Source);
    }

    internal void Activate(object? source)
    {
        if (_model is null || !control.IsEffectivelyEnabled) return;
        for (Visual? current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            DockingManager? manager = current switch
            {
                DockingManager owner => owner,
                LayoutFloatingWindowControl floating => floating.Model.Root?.Manager,
                _ => null
            };
            if (manager is null) continue;
            if (!ReferenceEquals(manager, _model.Root?.Manager)) return;
            break;
        }
        _model.IsActive = true;
    }

    private void ModelChanged(PropertyChangedEventArgs args)
    {
        if (_model is not { } model) return;
        if (args.PropertyName is nameof(LayoutContent.Parent) or nameof(LayoutContent.Root)) RefreshItem();
        if (args.PropertyName != nameof(LayoutContent.IsEnabled)) return;
        control.SetCurrentValue(InputElement.IsEnabledProperty, model.IsEnabled);
        if (model.IsEnabled || !model.IsActive || model.Parent is not ILayoutContentSelector selector ||
            model.Parent is not ILayoutGroup group) return;
        int enabled = -1;
        int index = 0;
        foreach (ILayoutElement child in group.Children)
        {
            if (child is LayoutContent { IsEnabled: true }) { enabled = index; break; }
            index++;
        }
        selector.SelectedContentIndex = enabled;
    }

    private void ItemDetached()
    {
        if (IsNativePresentation)
        {
            clearModel();
            control.SetCurrentValue(TemplatedControl.DataContextProperty, null);
            // A collapsed native tool pane can retain its deferred child after
            // removal. A retained public control must not retain that old tree.
            if (control.GetVisualParent() is ContentPresenter presenter && ReferenceEquals(presenter.Child, control))
            {
                presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
                presenter.UpdateChild();
            }
        }
        else RefreshItem();
    }

    private sealed class Subscription : IDisposable
    {
        private readonly LayoutContent _model;
        private readonly WeakReference<LayoutContentControlBinding> _owner;
        private LayoutItem? _item;

        internal Subscription(LayoutContent model, LayoutContentControlBinding owner)
        {
            _model = model;
            _owner = new(owner);
            model.PropertyChanged += ModelChanged;
        }
        internal void SetItem(LayoutItem? item)
        {
            if (ReferenceEquals(_item, item)) return;
            if (_item is not null) _item.PropertyChanged -= ItemChanged;
            _item = item;
            if (item is not null) item.PropertyChanged += ItemChanged;
        }
        private void ModelChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (_owner.TryGetTarget(out LayoutContentControlBinding? owner)) owner.ModelChanged(args);
            else Dispose();
        }
        private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutContentControlBinding? owner)) { Dispose(); return; }
            if (args.Property == LayoutItem.LayoutElementProperty && args.NewValue is null) owner.ItemDetached();
        }
        public void Dispose()
        {
            _model.PropertyChanged -= ModelChanged;
            SetItem(null);
        }
    }
}
