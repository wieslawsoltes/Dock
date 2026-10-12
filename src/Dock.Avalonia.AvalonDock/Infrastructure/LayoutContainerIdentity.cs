using System;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutContainerIdentity
{
    internal static bool IsPane(ILayoutElement element) =>
        element is LayoutDocumentPane or LayoutAnchorablePane or LayoutAnchorGroup;

    internal static void EnsureReferencedId(ILayoutContainer? container)
    {
        if (container is LayoutElement element && IsPane(element) && element.XmlId is null)
            element.DockModel.Id = Guid.NewGuid().ToString();
    }
}
