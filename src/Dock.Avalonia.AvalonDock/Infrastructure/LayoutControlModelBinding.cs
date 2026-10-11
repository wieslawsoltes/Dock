using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Resolves the public model and its existing container without allowing model
// publishers to retain discarded presentation controls.
internal sealed class LayoutControlModelBinding(TemplatedControl control,
    Action<LayoutItem?> setItem, Action clearModel,
    Action<LayoutContent, PropertyChangedEventArgs>? modelChanged = null)
{
    private Subscription? _subscription;
    internal LayoutContent? Model { get; private set; }
    internal bool IsNativePresentation { get; set; }

    internal void SetModel(LayoutContent? model)
    {
        _subscription?.Dispose();
        Model = model;
        _subscription = model is null ? null : new Subscription(model, this);
        RefreshItem();
    }

    internal void RefreshItem()
    {
        LayoutItem? item = Model?.DockModel.Factory is LayoutFactory factory
            ? factory.GetLayoutItem(Model.DockModel)
            : Model?.Root?.Manager?.GetLayoutItemFromModel(Model);
        setItem(item);
    }

    internal void WatchItem(LayoutItem? item) => _subscription?.SetItem(item);

    private void ModelChanged(PropertyChangedEventArgs args)
    {
        if (Model is not { } model) return;
        if (args.PropertyName is nameof(LayoutContent.Parent) or nameof(LayoutContent.Root)) RefreshItem();
        modelChanged?.Invoke(model, args);
    }

    private void ItemDetached()
    {
        if (IsNativePresentation)
        {
            clearModel();
            control.SetCurrentValue(TemplatedControl.DataContextProperty, null);
            // A collapsed native pane can retain its deferred child after
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
        private readonly WeakReference<LayoutControlModelBinding> _owner;
        private LayoutItem? _item;

        internal Subscription(LayoutContent model, LayoutControlModelBinding owner)
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
            if (_owner.TryGetTarget(out LayoutControlModelBinding? owner)) owner.ModelChanged(args);
            else Dispose();
        }

        private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutControlModelBinding? owner)) { Dispose(); return; }
            if (args.Property == LayoutItem.LayoutElementProperty && args.NewValue is null) owner.ItemDetached();
        }

        public void Dispose()
        {
            _model.PropertyChanged -= ModelChanged;
            SetItem(null);
        }
    }
}
