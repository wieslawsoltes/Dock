using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>A toggle button that opens a context menu below itself with an explicit data context.</summary>
public class DropDownButton : ToggleButton
{
    /// <summary>Defines the menu opened by the button.</summary>
    public static readonly StyledProperty<ContextMenu?> DropDownContextMenuProperty =
        AvaloniaProperty.Register<DropDownButton, ContextMenu?>(nameof(DropDownContextMenu));
    /// <summary>Defines the data context assigned when the menu opens.</summary>
    public static readonly StyledProperty<object?> DropDownContextMenuDataContextProperty =
        AvaloniaProperty.Register<DropDownButton, object?>(nameof(DropDownContextMenuDataContext));

    private readonly LayoutDropDownMenu _menu;

    /// <summary>Creates a dropdown button using the current Avalonia toggle-button theme.</summary>
    public DropDownButton() => _menu = new(this, () => SetCurrentValue(IsCheckedProperty, false));

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToggleButton);
    /// <summary>Gets or sets the menu displayed below the button.</summary>
    public ContextMenu? DropDownContextMenu { get => GetValue(DropDownContextMenuProperty); set => SetValue(DropDownContextMenuProperty, value); }
    /// <summary>Gets or sets the data context used for the next menu invocation.</summary>
    public object? DropDownContextMenuDataContext { get => GetValue(DropDownContextMenuDataContextProperty); set => SetValue(DropDownContextMenuDataContextProperty, value); }

    /// <summary>Releases an open menu when the menu property is replaced.</summary>
    protected virtual void OnDropDownContextMenuChanged(AvaloniaPropertyChangedEventArgs e) => _menu.Close();

    /// <summary>Opens the configured menu, then invokes the normal toggle-button click and command.</summary>
    protected override void OnClick()
    {
        if (!IsEffectivelyEnabled) return;
        ContextMenu? menu = DropDownContextMenu;
        if (menu is not null) _menu.Show(menu, DropDownContextMenuDataContext);
        base.OnClick();
        if (menu is not null) SetCurrentValue(IsCheckedProperty, _menu.IsOpen);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DropDownContextMenuProperty) OnDropDownContextMenuChanged(change);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _menu.Close();
        base.OnDetachedFromVisualTree(e);
    }
}
