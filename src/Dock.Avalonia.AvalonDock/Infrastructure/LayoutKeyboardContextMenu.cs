using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutKeyboardContextMenu
{
    public static bool IsShortcut(KeyEventArgs args) =>
        args.Key == Key.F10 && (args.KeyModifiers & KeyModifiers.Shift) != 0;

    public static bool HasContentMenu(object? source, DockingManager manager)
    {
        for (Visual? current = source as Visual; current is not null && !ReferenceEquals(current, manager); current = current.GetVisualParent())
            if (current is Control control && (control.ContextMenu is not null || control.ContextFlyout is not null)) return true;
        return false;
    }

    public static void Show(DockingManager manager, LayoutContextMenus menus)
    {
        object? active = manager.ActiveContent;
        if (active is LayoutContent content) active = content.Content;
        if (active is not AvaloniaObject) return;
        if (active is not Visual visual) throw new InvalidOperationException("The active content is not an Avalonia visual.");

        for (Visual? current = visual.GetVisualParent(); current is not null; current = current.GetVisualParent())
        {
            if (current is LayoutDocumentControl document)
            {
                if (manager.DocumentContextMenu is { } menu)
                    menus.Show(menu, document, document.LayoutItem, null, null, placeAtOrigin: true);
                return;
            }
            if (current is LayoutAnchorableControl anchorable)
            {
                if (manager.AnchorableContextMenu is { } menu)
                    menus.Show(menu, anchorable, anchorable.LayoutItem, null, null, placeAtOrigin: true);
                return;
            }
        }
    }
}
