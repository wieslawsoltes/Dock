using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutContextMenuRouting : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<LayoutContextMenuRouting, Control, bool>("IsEnabled");

    static LayoutContextMenuRouting()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, change) =>
        {
            if (change.GetNewValue<bool>()) control.AddHandler(Control.ContextRequestedEvent, Requested);
            else control.RemoveHandler(Control.ContextRequestedEvent, Requested);
        });
        Button.ClickEvent.AddClassHandler<Button>(MenuClicked);
    }

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);

    private static void Requested(object? sender, ContextRequestedEventArgs e)
    {
        if (e is LayoutContextMenuRequest) return;
        bool pointer = e.TryGetPosition(null, out _);
        for (Visual? current = e.Source as Visual; current is not null && !ReferenceEquals(current, sender); current = current.GetVisualParent())
        {
            if (current is not Control control) continue;
            // A content control owns its request even if its menu's Opening
            // handler cancels it. Do not substitute a docking menu afterward.
            if (control.ContextMenu is not null || control.ContextFlyout is not null) return;
            if (control is DocumentTabStripItem or ToolTabStripItem or ToolPinItemControl)
            {
                e.Handled = Show(control, control.DataContext as IDockable,
                    control is DocumentTabStripItem ? LayoutHeaderKind.Document : LayoutHeaderKind.Anchorable, e);
                return;
            }
            if (control.Name == "PART_Border" && control.TemplatedParent is ToolChromeControl chrome)
            {
                e.Handled = ShowPane(chrome, LayoutHeaderKind.Anchorable, e);
                return;
            }
            if (!pointer && control is DocumentControl or ToolControl)
            {
                e.Handled = ShowPane(control, control is DocumentControl ? LayoutHeaderKind.Document : LayoutHeaderKind.Anchorable, e);
                return;
            }
        }
    }

    private static void MenuClicked(Button button, RoutedEventArgs e)
    {
        if (button.Name != "PART_MenuButton" || button.TemplatedParent is not ToolChromeControl chrome ||
            !HasEnabledHost(button)) return;
        if (chrome.DataContext is IDock pane)
            e.Handled = Show(button, pane.ActiveDockable, LayoutHeaderKind.Anchorable, null, pane);
    }

    private static bool HasEnabledHost(Control control)
    {
        for (Visual? ancestor = control.GetVisualParent(); ancestor is not null; ancestor = ancestor.GetVisualParent())
            if (ancestor is Control host && GetIsEnabled(host)) return true;
        return false;
    }

    private static bool ShowPane(Control control, LayoutHeaderKind kind, ContextRequestedEventArgs? request) =>
        control.DataContext is IDock pane && Show(control, pane.ActiveDockable, kind, request, pane);

    private static bool Show(Control target, IDockable? model, LayoutHeaderKind kind, ContextRequestedEventArgs? request, IDock? pane = null)
    {
        if (model?.Factory is not LayoutFactory factory || factory.GetLayoutItem(model) is not LayoutItem { Manager: { } manager } item) return false;
        manager.ShowContextMenu(target, item, kind, request, pane);
        return true;
    }
}
