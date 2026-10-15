using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents a live tool pane using Dock's tool tabs and content presenter.</summary>
public class LayoutAnchorablePaneControl : LayoutCachePaneControl, ILayoutControl
{
    private readonly LayoutPaneControlBinding _binding;

    static LayoutAnchorablePaneControl() => FocusableProperty.OverrideDefaultValue<LayoutAnchorablePaneControl>(false);

    /// <summary>Creates a control for an existing tool pane.</summary>
    /// <param name="model">The public pane to display.</param>
    public LayoutAnchorablePaneControl(LayoutAnchorablePane model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;
        _binding = new(this, model, model, model.Children, false);
        _binding.Initialize();
    }

    /// <summary>Gets the public tool pane represented by this control.</summary>
    public ILayoutElement Model { get; }
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorablePaneControl);

    /// <inheritdoc/>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        _binding?.SelectFromControl(e, activate: false);
    }

    /// <summary>Activates the selected tool when keyboard focus enters this pane.</summary>
    protected virtual void OnGotKeyboardFocus(FocusChangedEventArgs e) => _binding.ActivateSelectedContent();
    /// <summary>Activates the selected tool on an unhandled left-button press within its owning manager.</summary>
    protected virtual void OnMouseLeftButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(e.Source, e.Handled);
    /// <summary>Activates the selected tool on an unhandled right-button press within its owning manager.</summary>
    protected virtual void OnMouseRightButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(e.Source, e.Handled);

    /// <inheritdoc/>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        OnGotKeyboardFocus(e);
        base.OnGotFocus(e);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonPressed: OnMouseLeftButtonDown(e); break;
            case PointerUpdateKind.RightButtonPressed: OnMouseRightButtonDown(e); break;
        }
    }
}
