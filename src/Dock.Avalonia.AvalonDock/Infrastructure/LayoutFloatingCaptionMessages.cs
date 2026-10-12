using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Native caption policy only. Avalonia and Dock retain movement, resize, hit
// testing, and execution of the system-menu's native window commands.
internal static class LayoutFloatingCaptionMessages
{
    private const int NonClientLeftButtonDown = 0x00A1;
    private const int NonClientRightButtonUp = 0x00A5;
    private const int Caption = 2;

    internal static void Process(LayoutFloatingWindowControl window, int message,
        IntPtr hitTest, IntPtr position, ref bool handled)
    {
        if (handled || message is not (NonClientLeftButtonDown or NonClientRightButtonUp) ||
            hitTest != (IntPtr)Caption || window.Model.Root?.Manager is not { } manager) return;
        if (message == NonClientLeftButtonDown)
        {
            if (window.Model is LayoutDocumentFloatingWindow { RootDocument: { } document }) document.IsActive = true;
            else
                foreach (ILayoutElement element in window.Model.Descendents())
                    if (element is ILayoutContentSelector { SelectedContent: { } selected })
                    {
                        selected.IsActive = true;
                        break;
                    }
            return;
        }
        if (message != NonClientRightButtonUp) return;

        LayoutItem? item = window switch
        {
            LayoutDocumentFloatingWindowControl document => document.RootDocumentLayoutItem,
            LayoutAnchorableFloatingWindowControl tool => tool.SingleContentLayoutItem,
            _ => null
        };
        LayoutHeaderKind kind = window.Model is LayoutDocumentFloatingWindow ? LayoutHeaderKind.Document : LayoutHeaderKind.Anchorable;
        ContextMenu? menu = kind == LayoutHeaderKind.Document ? manager.DocumentContextMenu : manager.AnchorableContextMenu;
        // LPARAM contains signed 16-bit physical screen coordinates.
        long packed = position.ToInt64();
        PixelPoint point = new(unchecked((short)packed), unchecked((short)(packed >> 16)));
        if (menu is not null && item is { IsAttached: true })
        {
            manager.ShowContextMenu(window, item, kind, null, item.LayoutElement?.DockModel.Owner as Dock.Model.Core.IDock,
                point);
            // A canceled custom menu still owns the request.
            handled = true;
        }
        else if (!manager.ShowSystemMenu) handled = true;
        else handled = window.TryShowSystemMenu(point);
    }
}
