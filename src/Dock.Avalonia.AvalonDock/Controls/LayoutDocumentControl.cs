using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents a document-pane item through its existing Dock content presenter.</summary>
public class LayoutDocumentControl : TemplatedControl
{
    /// <summary>Defines the public document or tool model displayed in a document pane.</summary>
    public static readonly StyledProperty<LayoutContent?> ModelProperty =
        AvaloniaProperty.Register<LayoutDocumentControl, LayoutContent?>(nameof(Model));
    /// <summary>Defines the manager's container for the displayed model.</summary>
    public static readonly DirectProperty<LayoutDocumentControl, LayoutItem?> LayoutItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutDocumentControl, LayoutItem?>(nameof(LayoutItem), control => control.LayoutItem);

    private readonly LayoutContentControlBinding _binding;
    private LayoutItem? _layoutItem;

    static LayoutDocumentControl() => FocusableProperty.OverrideDefaultValue<LayoutDocumentControl>(true);

    /// <summary>Creates a document content control.</summary>
    public LayoutDocumentControl()
    {
        _binding = new(this, SetLayoutItem, () => SetCurrentValue(ModelProperty, null), () => Model, anchorable: false);
        AddHandler(PointerPressedEvent, PreviewPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutDocumentControl);
    /// <summary>Gets or sets the public model displayed by this control.</summary>
    public LayoutContent? Model { get => GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    /// <summary>Gets the manager's existing item container, or null for unattached content.</summary>
    public LayoutItem? LayoutItem => _layoutItem;
    /// <summary>Updates model subscriptions, enablement, and the item container.</summary>
    /// <remarks>Removes one enablement subscription from the supplied old model and adds one
    /// to the current model. Replayed base calls retain their separate registrations.</remarks>
    protected virtual void OnModelChanged(AvaloniaPropertyChangedEventArgs e) => _binding.SetModel((LayoutContent?)e.OldValue, Model);
    /// <summary>Sets the container exposed to compiled control templates.</summary>
    protected void SetLayoutItem(LayoutItem? value)
    {
        SetAndRaise(LayoutItemProperty, ref _layoutItem, value);
        _binding?.WatchItem(_layoutItem);
    }
    /// <summary>Activates this content when keyboard focus enters it.</summary>
    protected virtual void OnPreviewGotKeyboardFocus(FocusChangedEventArgs e) => _binding.ActivateFromFocus(Model, e);
    /// <summary>Activates this content before a left-button press within its owning manager reaches its editor.</summary>
    protected virtual void OnPreviewMouseLeftButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(Model, e.Source);
    /// <summary>Activates this content before a right-button press within its owning manager reaches its editor.</summary>
    protected virtual void OnPreviewMouseRightButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(Model, e.Source);

    internal void SetNativeModel(LayoutContent model)
    {
        _binding.IsNativePresentation = true;
        SetCurrentValue(ModelProperty, model);
        _binding.RefreshItem();
    }

    private void PreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonPressed: OnPreviewMouseLeftButtonDown(e); break;
            case PointerUpdateKind.RightButtonPressed: OnPreviewMouseRightButtonDown(e); break;
        }
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        OnPreviewGotKeyboardFocus(e);
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
