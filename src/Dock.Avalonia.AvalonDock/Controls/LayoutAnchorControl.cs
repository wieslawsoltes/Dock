using System;
using Avalonia;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;

namespace AvalonDock.Controls;

/// <summary>Presents an auto-hidden tool using Dock's native pinned-item drag and preview support.</summary>
public class LayoutAnchorControl : ToolPinItemControl, ILayoutControl
{
    /// <summary>Identifies the edge containing this auto-hide anchor.</summary>
    public static readonly DirectProperty<LayoutAnchorControl, AnchorSide> SideProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorControl, AnchorSide>(nameof(Side), control => control.Side);

    private readonly LayoutAnchorable _model;
    private readonly LayoutAnchorInput _input;
    private readonly LayoutAnchorViewBinding _binding;
    private AnchorSide _side;

    internal LayoutAnchorControl(LayoutAnchorable model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        DataContext = model.DockModel;
        _input = new(this, model);
        _binding = new(this, model, DockingManager.AnchorTemplateProperty, ModelChanged);
        _binding.Initialize();
    }

    /// <summary>Gets the existing public auto-hidden tool.</summary>
    public ILayoutElement Model => _model;
    /// <summary>Gets the edge containing the tool's auto-hide group.</summary>
    public AnchorSide Side => _side;
    /// <summary>Updates the edge and the native item's orientation.</summary>
    protected void SetSide(AnchorSide value)
    {
        SetAndRaise(SideProperty, ref _side, value);
        SetCurrentValue(OrientationProperty, value is AnchorSide.Left or AnchorSide.Right
            ? global::Avalonia.Layout.Orientation.Vertical : global::Avalonia.Layout.Orientation.Horizontal);
    }
    /// <summary>Opens the tool's native preview and activates it for an unhandled pointer press.</summary>
    protected virtual void OnMouseDown(PointerPressedEventArgs e)
    {
        if (!e.Handled)
        {
            _input.Show(activate: true);
            // The native pinned-item helper tracks movement on this control.
            // Default anchors have no Button child to capture that movement.
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }
    }
    /// <summary>Schedules the native preview after the pointer rests over the anchor for 400 milliseconds.</summary>
    protected virtual void OnMouseEnter(PointerEventArgs e)
    {
        if (!e.Handled) _input.Enter();
    }
    /// <summary>Cancels a pending hover preview when the pointer leaves the anchor.</summary>
    protected virtual void OnMouseLeave(PointerEventArgs e) => _input.Cancel();
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorControl);
    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e) { base.OnPointerPressed(e); OnMouseDown(e); }
    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (ReferenceEquals(e.Pointer.Captured, this)) e.Pointer.Capture(null);
    }
    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e) { base.OnPointerEntered(e); OnMouseEnter(e); }
    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e) { base.OnPointerExited(e); OnMouseLeave(e); }
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.Initialize();
    }
    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _input.Cancel();
        base.OnDetachedFromVisualTree(e);
    }

    private void ModelChanged(string? property)
    {
        if (_model.FindParent<LayoutAnchorSide>() is { } side) SetSide(side.Side);
        _input.ModelChanged(property);
    }
}
