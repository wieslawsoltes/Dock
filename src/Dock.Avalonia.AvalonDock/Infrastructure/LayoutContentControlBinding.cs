using System;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Content controls add focus and enablement policies to the shared model binding.
internal sealed class LayoutContentControlBinding
{
    private readonly TemplatedControl _control;
    private readonly LayoutControlModelBinding _model;
    private readonly LayoutContentPreviewRegistration _preview;
    private readonly LayoutContentModelEvents _events;
    private readonly Action<LayoutItem?> _setItem;
    private readonly Func<LayoutContent?> _getModel;
    private readonly bool _anchorable;
    private LayoutContent? _publicModel;
    private bool _publicModelSubscribed;

    internal LayoutContentControlBinding(TemplatedControl control, Action<LayoutItem?> setItem,
        Action clearModel, Func<LayoutContent?> getModel, bool anchorable)
    {
        _control = control;
        _setItem = setItem;
        _getModel = getModel;
        _anchorable = anchorable;
        _preview = new(control);
        _model = new(control, setItem, clearModel, ModelChanged);
        _events = new(EnablementChanged);
    }

    internal bool IsNativePresentation { get => _model.IsNativePresentation; set => _model.IsNativePresentation = value; }
    internal void SetModel(LayoutContent? previous, LayoutContent? model)
    {
        _events.Remove(previous);
        if (IsNativePresentation)
        {
            _publicModel = null;
            if (model is not null) _events.Add(model);
            SetNativeModel(model);
            return;
        }

        _publicModel = model;
        _publicModelSubscribed = model is not null && (!_anchorable || model.Root?.Manager is not null);
        if (_publicModelSubscribed)
        {
            _events.Add(model!);
            // The document hook subscribes before resolving its manager, and
            // retains that subscription if an unattached model causes failure.
            _setItem(model!.Root!.Manager!.GetLayoutItemFromModel(model));
        }
        else _setItem(null);
    }

    private void SetNativeModel(LayoutContent? model)
    {
        _model.SetModel(model);
        _preview.Refresh(_model.Model, native: true);
        if (ReferenceEquals(_model.Model, model))
            _control.SetCurrentValue(InputElement.IsEnabledProperty, model?.IsEnabled ?? true);
    }

    internal void RefreshItem()
    {
        if (!IsNativePresentation) return;
        // A custom compiled template can assign Model before the native host
        // adopts its control. Upgrade only a model whose base hook actually ran.
        if (_publicModel is { } model)
        {
            if (ReferenceEquals(model, _getModel()))
            {
                _publicModel = null;
                if (!_publicModelSubscribed) _events.Add(model);
                SetNativeModel(model);
            }
            return;
        }
        _model.RefreshItem();
    }
    internal void WatchItem(LayoutItem? item) => _model.WatchItem(item);

    internal void ActivateFromFocus(LayoutContent? model, FocusChangedEventArgs args)
    {
        if (args.NewFocusedElement is not null && args.OldFocusedElement is LayoutFloatingWindowControl) return;
        // The native deferred presenter can still show the previous selection
        // while a modal owner restores focus. Do not undo the new selection.
        if (IsNativePresentation && model?.IsSelected != true && args.NavigationMethod == NavigationMethod.Unspecified) return;
        if (model is not null) model.IsActive = true;
    }

    internal void ActivateFromPointer(LayoutContent? model, object? source)
    {
        if (model is not null && LayoutInputOwnership.IsWithinManager(source, model.Root?.Manager))
            model.IsActive = true;
    }

    private void ModelChanged(LayoutContent model, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(LayoutContent.Root) or nameof(LayoutContent.Parent) or nameof(Dock.Model.Core.IDockable.Factory))
            _preview.Refresh(model, IsNativePresentation);
    }

    private void EnablementChanged()
    {
        if (_getModel() is not { } model) return;
        _control.SetCurrentValue(InputElement.IsEnabledProperty, model.IsEnabled);
        // Application handlers can replace Model while enablement is published.
        // As in the compatibility hook, subsequent decisions use its live value.
        if (_control.IsEnabled || _getModel() is not { IsActive: true } current) return;
        ILayoutContainer? parent = current.Parent;
        if (_anchorable ? parent is not LayoutAnchorablePane : parent is not LayoutDocumentPane) return;
        ILayoutContentSelector selector = (ILayoutContentSelector)parent;
        ILayoutGroup group = (ILayoutGroup)parent;
        selector.SelectedContentIndex = -1;
        int index = 0;
        foreach (ILayoutElement child in group.Children)
        {
            if (child is LayoutContent { IsEnabled: true })
            {
                selector.SelectedContentIndex = index;
                return;
            }
            index++;
        }
    }
}
