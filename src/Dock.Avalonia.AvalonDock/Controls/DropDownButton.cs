using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
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

    /// <summary>Opens the configured menu, toggles the button, raises Click, and invokes the current command when it can execute.</summary>
    protected override void OnClick()
    {
        ContextMenu? menu = DropDownContextMenu;
        if (menu is not null) _menu.Show(menu, DropDownContextMenuDataContext);
        // WPF's protected hook also runs when explicitly called while disabled,
        // and a handled Click event does not suppress its command.
        Toggle();
        RaiseEvent(new RoutedEventArgs(ClickEvent));
        object? parameter = CommandParameter;
        if (Command is { } command && command.CanExecute(parameter)) command.Execute(parameter);
        if (menu is not null && !_menu.IsOpen) SetCurrentValue(IsCheckedProperty, false);
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
