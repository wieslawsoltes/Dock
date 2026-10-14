using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents an anchorable through its existing Dock content presenter.</summary>
public class LayoutAnchorableControl : TemplatedControl
{
    /// <summary>Defines the public tool model displayed by this control.</summary>
    public static readonly StyledProperty<LayoutAnchorable?> ModelProperty =
        AvaloniaProperty.Register<LayoutAnchorableControl, LayoutAnchorable?>(name: nameof(Model));
    /// <summary>Defines the manager's container for the displayed model.</summary>
    public static readonly DirectProperty<LayoutAnchorableControl, LayoutItem?> LayoutItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorableControl, LayoutItem?>(nameof(LayoutItem), control => control.LayoutItem);

    private readonly LayoutContentControlBinding _binding;
    private LayoutItem? _layoutItem;

    /// <summary>Creates an anchorable content control.</summary>
    public LayoutAnchorableControl() => _binding = new(this, SetLayoutItem,
        () => SetCurrentValue(ModelProperty, null), () => Model, anchorable: true);
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorableControl);
    /// <summary>Gets or sets the public tool model displayed by this control.</summary>
    public LayoutAnchorable? Model { get => GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    /// <summary>Gets the item container resolved by the last model-change hook, or null for unattached content.</summary>
    public LayoutItem? LayoutItem => _layoutItem;
    /// <summary>Updates model subscriptions and the item container.</summary>
    /// <remarks>Removes one enablement subscription from the supplied old model. Each base
    /// call subscribes to the current managed model; callbacks read the live Model property.
    /// Enablement changes when the model later raises its IsEnabled notification.</remarks>
    protected virtual void OnModelChanged(AvaloniaPropertyChangedEventArgs e) => _binding.SetModel((LayoutContent?)e.OldValue, Model);
    /// <summary>Sets the container exposed to compiled control templates.</summary>
    protected void SetLayoutItem(LayoutItem? value)
    {
        SetAndRaise(LayoutItemProperty, ref _layoutItem, value);
        _binding?.WatchItem(_layoutItem);
    }
    /// <summary>Activates this tool when keyboard focus enters its content.</summary>
    protected virtual void OnGotKeyboardFocus(FocusChangedEventArgs e) => _binding.ActivateFromFocus(Model, e);

    internal void SetNativeModel(LayoutAnchorable model)
    {
        _binding.IsNativePresentation = true;
        SetCurrentValue(ModelProperty, model);
        _binding.RefreshItem();
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        OnGotKeyboardFocus(e);
        base.OnGotFocus(e);
    }
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.RefreshItem();
    }
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ModelProperty) OnModelChanged(change);
    }
}
