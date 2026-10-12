using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents a live document pane using Dock's document tabs and content presenter.</summary>
public class LayoutDocumentPaneControl : LayoutCachePaneControl, ILayoutControl
{
    private readonly LayoutPaneControlBinding _binding;

    static LayoutDocumentPaneControl() => FocusableProperty.OverrideDefaultValue<LayoutDocumentPaneControl>(false);

    internal LayoutDocumentPaneControl(LayoutDocumentPane model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;
        _binding = new(this, model, model, model.Children, true);
        _binding.Initialize();
    }

    /// <summary>Gets the public document pane represented by this control.</summary>
    public ILayoutElement Model { get; }
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutDocumentPaneControl);

    /// <summary>Applies an explicit control selection and activates the selected document.</summary>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        _binding?.SelectFromControl(e, activate: true);
    }

    /// <summary>Activates the selected content on an unhandled left-button press within its owning manager.</summary>
    protected virtual void OnMouseLeftButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(e.Source, e.Handled);
    /// <summary>Activates the selected content on an unhandled right-button press within its owning manager.</summary>
    protected virtual void OnMouseRightButtonDown(PointerPressedEventArgs e) => _binding.ActivateFromPointer(e.Source, e.Handled);

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
