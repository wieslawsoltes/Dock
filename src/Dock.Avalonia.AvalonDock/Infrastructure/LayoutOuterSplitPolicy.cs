using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal static class LayoutOuterSplitPolicy
{
    internal static IDock? Resolve(IDockable source, LayoutElement target, DockOperation operation)
    {
        LayoutDocumentPane? pane = target as LayoutDocumentPane ?? target.Parent as LayoutDocumentPane;
        if (pane is null || !ContainsOnlyTools(source)) return null;
        if (pane.Root?.Manager?.AllowMixedOrientation != true && pane.Parent is LayoutDocumentPaneGroup group)
        {
            int visibleCount = 0, paneIndex = -1;
            for (int index = 0; index < group.Children.Count; index++)
            {
                ILayoutDocumentPane child = group.Children[index];
                if (!child.IsVisible) continue;
                if (ReferenceEquals(child, pane)) paneIndex = visibleCount;
                visibleCount++;
            }
            if (visibleCount > 1)
            {
                bool horizontal = group.Orientation == global::Avalonia.Layout.Orientation.Horizontal;
                bool available = operation switch
                {
                    DockOperation.OuterLeft => horizontal && paneIndex == 0,
                    DockOperation.OuterRight => horizontal && paneIndex == group.ChildrenCount - 1,
                    DockOperation.OuterTop => !horizontal && paneIndex == 0,
                    DockOperation.OuterBottom => !horizontal && paneIndex == group.ChildrenCount - 1,
                    _ => false
                };
                if (!available) return null;
            }
        }
        LayoutElement region = pane;
        while (region.Parent is LayoutDocumentPaneGroup parent) region = parent;
        return region.Parent is LayoutPanel ? (IDock)region.DockModel : null;
    }

    private static bool ContainsOnlyTools(IDockable source)
    {
        if (source is ITool) return true;
        if (source is not IDock { VisibleDockables: { Count: > 0 } children }) return false;
        bool hasTool = false;
        for (int index = 0; index < children.Count; index++)
        {
            IDockable child = children[index];
            if (child is ISplitter) continue;
            if (!ContainsOnlyTools(child)) return false;
            hasTool = true;
        }
        return hasTool;
    }
}
