using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Project the public visibility rules without editing the public or native tree.
internal static class LayoutDragSourceVisibility
{
    internal static bool IsVisible(ILayoutPanelElement element, ILayoutPanelElement removed)
    {
        if (ReferenceEquals(element, removed))
            return element is LayoutDocumentPane && element.Parent is not LayoutDocumentPaneGroup;
        if (!element.IsVisible) return false;
        if (element is LayoutDocumentPaneGroup) return true;
        if (element is LayoutPanel or LayoutAnchorablePaneGroup)
        {
            foreach (ILayoutElement child in ((ILayoutContainer)element).Children)
                if (child is ILayoutPanelElement panel && IsVisible(panel, removed)) return true;
            return false;
        }
        return true;
    }
}
