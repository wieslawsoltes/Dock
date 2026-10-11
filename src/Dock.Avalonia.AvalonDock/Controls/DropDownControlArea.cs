using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>A content area that opens a menu at the pointer on an unhandled right-button release.</summary>
public class DropDownControlArea : UserControl
{
    /// <summary>Defines the menu opened by the area.</summary>
    public static readonly StyledProperty<ContextMenu?> DropDownContextMenuProperty =
        AvaloniaProperty.Register<DropDownControlArea, ContextMenu?>(nameof(DropDownContextMenu));
    /// <summary>Defines the data context assigned when the menu opens.</summary>
    public static readonly StyledProperty<object?> DropDownContextMenuDataContextProperty =
        AvaloniaProperty.Register<DropDownControlArea, object?>(nameof(DropDownContextMenuDataContext));

    private readonly LayoutDropDownMenu _menu;

    /// <summary>Creates a content area using Avalonia pointer routing.</summary>
    public DropDownControlArea()
    {
        _menu = new(this);
        AddHandler(PointerReleasedEvent, PreviewReleased, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(UserControl);
    /// <summary>Gets or sets the menu displayed at the pointer.</summary>
    public ContextMenu? DropDownContextMenu { get => GetValue(DropDownContextMenuProperty); set => SetValue(DropDownContextMenuProperty, value); }
    /// <summary>Gets or sets the data context used for the next menu invocation.</summary>
    public object? DropDownContextMenuDataContext { get => GetValue(DropDownContextMenuDataContextProperty); set => SetValue(DropDownContextMenuDataContextProperty, value); }

    /// <summary>Receives right-button presses routed through this area.</summary>
    protected virtual void OnMouseRightButtonDown(PointerPressedEventArgs e) { }
    /// <summary>Opens the configured menu on the pointer-release tunnel route.</summary>
    protected virtual void OnPreviewMouseRightButtonUp(PointerReleasedEventArgs e)
    {
        if (e.Handled || !IsEffectivelyEnabled || DropDownContextMenu is not { } menu) return;
        for (Visual? current = e.Source as Visual; current is not null && !ReferenceEquals(current, this); current = current.GetVisualParent())
            if (current is Control control && (control.ContextMenu is not null || control.ContextFlyout is not null ||
                control is DropDownControlArea { DropDownContextMenu: not null })) return;
        _menu.Show(menu, DropDownContextMenuDataContext, e);
        // Prevent Avalonia's subsequent gesture from opening a second menu.
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) OnMouseRightButtonDown(e);
    }

    private void PreviewReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right) OnPreviewMouseRightButtonUp(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DropDownContextMenuProperty) _menu.Close();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _menu.Close();
        base.OnDetachedFromVisualTree(e);
    }
}
