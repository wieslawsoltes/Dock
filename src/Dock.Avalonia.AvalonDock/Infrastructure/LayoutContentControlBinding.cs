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

    internal LayoutContentControlBinding(TemplatedControl control, Action<LayoutItem?> setItem, Action clearModel)
    {
        _control = control;
        _preview = new(control);
        _model = new(control, setItem, clearModel, ModelChanged);
    }

    internal bool IsNativePresentation { get => _model.IsNativePresentation; set => _model.IsNativePresentation = value; }
    internal void SetModel(LayoutContent? model)
    {
        _model.SetModel(model);
        _preview.Refresh(_model.Model, IsNativePresentation);
        if (ReferenceEquals(_model.Model, model))
            _control.SetCurrentValue(InputElement.IsEnabledProperty, model?.IsEnabled ?? true);
    }
    internal void RefreshItem() => _model.RefreshItem();
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
        if (args.PropertyName != nameof(LayoutContent.IsEnabled)) return;
        _control.SetCurrentValue(InputElement.IsEnabledProperty, model.IsEnabled);
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
}
