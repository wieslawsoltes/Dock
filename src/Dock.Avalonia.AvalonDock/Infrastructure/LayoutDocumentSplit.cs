using AvalonDock.Layout;
using Dock.Model.Core;
using Orientation = Avalonia.Layout.Orientation;

namespace AvalonDock.Infrastructure;

// Keep document splits inside a typed document region. Dock still creates the
// pane, moves its contents and performs the split on the same native models.
internal static class LayoutDocumentSplit
{
    internal static void Prepare(LayoutDocumentPane pane, DockOperation operation)
    {
        if (operation is not (DockOperation.Left or DockOperation.Top or DockOperation.Right or DockOperation.Bottom) ||
            pane.Parent is not { } parent) return;
        Orientation orientation = operation is DockOperation.Left or DockOperation.Right
            ? Orientation.Horizontal : Orientation.Vertical;
        if (parent is LayoutDocumentPaneGroup group)
        {
            if (pane.Root?.Manager?.AllowMixedOrientation != true) group.Orientation = orientation;
            return;
        }

        group = new LayoutDocumentPaneGroup { Orientation = orientation };
        parent.ReplaceChild(pane, group);
        group.Children.Add(pane);
    }
}
