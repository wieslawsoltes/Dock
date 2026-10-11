using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents a tool title using its existing layout item and Dock's pane/window dragging.</summary>
public class AnchorablePaneTitle : TemplatedControl
{
    /// <summary>Defines the public tool whose title is displayed.</summary>
    public static readonly StyledProperty<LayoutAnchorable?> ModelProperty =
        AvaloniaProperty.Register<AnchorablePaneTitle, LayoutAnchorable?>(nameof(Model));
    /// <summary>Defines the manager's existing container for the displayed tool.</summary>
    public static readonly DirectProperty<AnchorablePaneTitle, LayoutItem?> LayoutItemProperty =
        AvaloniaProperty.RegisterDirect<AnchorablePaneTitle, LayoutItem?>(nameof(LayoutItem), title => title.LayoutItem);

    private readonly LayoutControlModelBinding _binding;
    private readonly LayoutAnchorableTitleInput _input;
    private LayoutItem? _layoutItem;

    /// <summary>Creates a nonfocusable tool title.</summary>
    public AnchorablePaneTitle()
    {
        _binding = new(this, SetLayoutItem, () => SetCurrentValue(ModelProperty, null));
        _input = new(this);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(AnchorablePaneTitle);
    /// <summary>Gets or sets the public tool model.</summary>
    public LayoutAnchorable? Model { get => GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    /// <summary>Gets the manager's current item container, or null for unattached content.</summary>
    public LayoutItem? LayoutItem => _layoutItem;
    /// <summary>Updates the model/container connection and cancels an unfinished title press.</summary>
    protected virtual void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _input.Cancel();
        _binding.SetModel(Model);
    }
    /// <summary>Sets the container available to compiled title templates.</summary>
    protected void SetLayoutItem(LayoutItem? value)
    {
        SetAndRaise(LayoutItemProperty, ref _layoutItem, value);
        _binding?.WatchItem(_layoutItem);
    }
    /// <summary>Tracks a title press, using the native grip when hosted in Dock tool chrome.</summary>
    protected virtual void OnMouseLeftButtonDown(PointerPressedEventArgs e) => _input.Pressed(e);
    /// <summary>Activates the tool when a title press is released.</summary>
    protected virtual void OnMouseLeftButtonUp(PointerReleasedEventArgs e) => _input.Released(e);
    /// <summary>Cancels an unfinished title press when the left button is no longer down.</summary>
    protected virtual void OnMouseMove(PointerEventArgs e) => _input.Moved(e);
    /// <summary>Floats a custom title's pane through Dock when its pressed pointer leaves the title.</summary>
    protected virtual void OnMouseLeave(PointerEventArgs e) => _input.Exited(e);

    internal void SetNativeModel(LayoutAnchorable model)
    {
        _binding.IsNativePresentation = true;
        SetCurrentValue(ModelProperty, model);
        _binding.RefreshItem();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) OnMouseLeftButtonDown(e);
    }
    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left) OnMouseLeftButtonUp(e);
    }
    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        OnMouseMove(e);
    }
    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        OnMouseLeave(e);
    }
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.RefreshItem();
    }
    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _input.Cancel();
        base.OnDetachedFromVisualTree(e);
    }
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ModelProperty) OnModelChanged(change);
    }
}
