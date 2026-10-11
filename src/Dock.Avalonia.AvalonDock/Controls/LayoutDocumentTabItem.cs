using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents a document tab header using its existing layout item and native Dock operations.</summary>
public class LayoutDocumentTabItem : TemplatedControl
{
    /// <summary>Defines the public content displayed by the tab.</summary>
    public static readonly StyledProperty<LayoutContent?> ModelProperty =
        AvaloniaProperty.Register<LayoutDocumentTabItem, LayoutContent?>(nameof(Model));
    /// <summary>Defines the manager's existing item container for this tab.</summary>
    public static readonly DirectProperty<LayoutDocumentTabItem, LayoutItem?> LayoutItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutDocumentTabItem, LayoutItem?>(nameof(LayoutItem), tab => tab.LayoutItem);

    private readonly LayoutControlModelBinding _binding;
    private readonly LayoutTabItemInput _input;
    private LayoutItem? _layoutItem;

    /// <summary>Creates a nonfocusable document tab header.</summary>
    public LayoutDocumentTabItem()
    {
        _binding = new(this, SetLayoutItem, () => SetCurrentValue(ModelProperty, null));
        _input = new(this, () => Model, () => LayoutItem, true);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutDocumentTabItem);
    /// <summary>Gets or sets the public document or tool model.</summary>
    public LayoutContent? Model { get => GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    /// <summary>Gets the manager's current item container, or null for unattached content.</summary>
    public LayoutItem? LayoutItem => _layoutItem;
    /// <summary>Updates the model/container connection and cancels an unfinished tab drag.</summary>
    protected virtual void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _input.Cancel();
        _binding.SetModel(Model);
    }
    /// <summary>Sets the container available to compiled header templates.</summary>
    protected void SetLayoutItem(LayoutItem? value)
    {
        SetAndRaise(LayoutItemProperty, ref _layoutItem, value);
        _binding?.WatchItem(_layoutItem);
    }
    /// <summary>Starts a custom tab press while preserving the native tab's drag handling.</summary>
    protected virtual void OnMouseLeftButtonDown(PointerPressedEventArgs e) => _input.Pressed(e);
    /// <summary>Ends a custom tab drag and applies this tab kind's activation policy.</summary>
    protected virtual void OnMouseLeftButtonUp(PointerReleasedEventArgs e) => _input.Released(e);
    /// <summary>Reorders custom tabs or floats content through Dock after crossing the drag threshold.</summary>
    protected virtual void OnMouseMove(PointerEventArgs e) => _input.Moved(e);
    /// <summary>Clears an unfinished press when an uncaptured pointer leaves the tab.</summary>
    protected virtual void OnMouseLeave(PointerEventArgs e) => _input.BoundaryChanged();
    /// <summary>Clears an unfinished press when an uncaptured pointer enters the tab.</summary>
    protected virtual void OnMouseEnter(PointerEventArgs e) => _input.BoundaryChanged();
    /// <summary>Invokes the current close command for a custom document tab on a middle-button press.</summary>
    protected virtual void OnMouseDown(PointerPressedEventArgs e) => _input.MouseDown(e);

    internal void SetNativeModel(LayoutContent model)
    {
        _binding.IsNativePresentation = true;
        SetCurrentValue(ModelProperty, model);
        _binding.RefreshItem();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        OnMouseDown(e);
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) OnMouseLeftButtonDown(e);
    }
    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left) OnMouseLeftButtonUp(e);
    }
    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e) { base.OnPointerMoved(e); OnMouseMove(e); }
    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e) { base.OnPointerExited(e); OnMouseLeave(e); }
    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e) { base.OnPointerEntered(e); OnMouseEnter(e); }
    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) { _input.Cancel(); base.OnPointerCaptureLost(e); }
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.RefreshItem();
    }
    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) { _input.Cancel(); base.OnDetachedFromVisualTree(e); }
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ModelProperty) OnModelChanged(change);
    }
}
