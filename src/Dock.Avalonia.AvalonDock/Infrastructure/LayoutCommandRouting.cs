using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Handles command gestures before native tabs invoke factory operations. All
// selection, dragging, and docking continue through the native controls.
internal sealed class LayoutCommandRouting : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<LayoutCommandRouting, DockControl, bool>("IsEnabled");

    static LayoutCommandRouting()
    {
        IsEnabledProperty.Changed.AddClassHandler<DockControl>(Changed);
        // DoubleTapped only bubbles. Class handlers run before native tab
        // instance handlers, including when the tab itself is the event source.
        InputElement.DoubleTappedEvent.AddClassHandler<DocumentTabStripItem>(DoubleTapped);
        InputElement.DoubleTappedEvent.AddClassHandler<ToolTabStripItem>(DoubleTapped);
        Button.ClickEvent.AddClassHandler<Button>(ToolCloseClicked);
    }

    public static bool GetIsEnabled(DockControl control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DockControl control, bool value) => control.SetValue(IsEnabledProperty, value);

    private static void Changed(DockControl control, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.GetNewValue<bool>())
        {
            control.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }
        else
        {
            control.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        }
    }

    private static LayoutItem? FindItem(RoutedEventArgs e, out IDockable? model)
    {
        for (Visual? visual = e.Source as Visual; visual is not null; visual = visual.GetVisualParent())
        {
            if (visual is DocumentTabStripItem or ToolTabStripItem)
            {
                model = ((Control)visual).DataContext as IDockable;
                return model?.Factory is LayoutFactory factory ? factory.GetLayoutItem(model) : null;
            }
            if (visual is DockControl) break;
        }
        model = null;
        return null;
    }

    private static void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DockControl control || !e.GetCurrentPoint(control).Properties.IsMiddleButtonPressed ||
            FindItem(e, out IDockable? model) is not { } item || model is null) return;
        e.Handled = true;
        if (DockCapabilityResolver.IsEnabled(model, DockCapability.Close,
                DockCapabilityResolver.ResolveOperationDock(model))) Execute(item.CloseCommand);
    }

    private static void DoubleTapped(Control sender, TappedEventArgs e)
    {
        if (!IsEnabled(sender)) return;
        if (FindItem(e, out IDockable? model) is not { } item || model is null) return;
        e.Handled = true;
        for (Visual? source = e.Source as Visual; source is not null && !ReferenceEquals(source, sender); source = source.GetVisualParent())
            if (source is Button) return;
        if (DockCapabilityResolver.IsEnabled(model, DockCapability.Float,
                DockCapabilityResolver.ResolveOperationDock(model)) &&
            model.Owner is IDock owner && (owner.CanCloseLastDockable || (owner.VisibleDockables?.Count ?? 0) > 1))
        {
            Execute(item.FloatCommand);
            if (item.LayoutElement?.IsFloating == true) model.Factory?.ActivateWindow(model);
        }
    }

    private static bool IsEnabled(Control control) =>
        control.FindAncestorOfType<DockControl>() is { } dock && GetIsEnabled(dock);

    private static void ToolCloseClicked(Button button, RoutedEventArgs e)
    {
        if (button.Name != "PART_CloseButton" || button.DataContext is not LayoutAnchorableItem || !IsEnabled(button)) return;
        // HostWindow otherwise closes a single-tool window in its Click handler
        // before Button executes the bound command. The command owns this action,
        // so application cancellation/customization must run first.
        e.Handled = true;
        Execute(button.Command);
    }

    private static void Execute(ICommand? command)
    {
        if (command?.CanExecute(null) == true) command.Execute(null);
    }
}
