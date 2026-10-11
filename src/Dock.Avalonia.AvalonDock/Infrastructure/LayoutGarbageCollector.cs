using System.Collections.Generic;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutGarbageCollector
{
    internal static void Collect(LayoutRoot root)
    {
        bool removed;
        do
        {
            removed = false;
            List<ILayoutElement> nodes = new(root.Descendents());
            HashSet<ILayoutContainer> referenced = new();
            int documentPanes = 0;
            foreach (ILayoutElement node in nodes)
            {
                if (node is LayoutDocumentPane) documentPanes++;
                if (node is LayoutContent content)
                {
                    if (content.PreviousContainer?.Root != root ||
                        (content.PreviousContainer is { ChildrenCount: 0 } previousEmpty &&
                         !LayoutDockingHistory.IsFloatingContainer(previousEmpty) && !content.IsFloating &&
                         content is not LayoutAnchorable { IsVisible: false }))
                    {
                        content.PreviousContainer = null;
                        content.PreviousContainerIndex = -1;
                        content.PreviousContainerId = null;
                    }
                    if (content.InitialContainer?.Root != root)
                    {
                        content.InitialContainer = null;
                        content.InitialContainerIndex = -1;
                        content.InitialContainerId = null;
                    }
                    if (content.PreviousContainer is { } previous) referenced.Add(previous);
                    if (content.InitialContainer is { } initial) referenced.Add(initial);
                }
                if (node is LayoutAnchorGroup group)
                {
                    if (group.PreviousContainer?.Root != root) group.PreviousContainer = null;
                    if (group.PreviousContainer is { } previous) referenced.Add(previous);
                }
            }
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i] is not ILayoutContainer { ChildrenCount: 0 } empty || empty is LayoutAnchorSide ||
                    ReferenceEquals(empty, root.RootPanel) || referenced.Contains(empty)) continue;
                if (empty is LayoutDocumentPane && documentPanes == 1) continue;
                if (empty.Parent is not { } parent) continue;
                parent.RemoveChild(empty);
                // A floating host can cancel removal. Retry only when this
                // pass changed the hierarchy, otherwise collection never ends.
                if (!ReferenceEquals(empty.Parent, parent))
                {
                    if (empty is LayoutDocumentPane) documentPanes--;
                    removed = true;
                }
            }
        } while (removed);
        if (root.ActiveContent is { } active && !ReferenceEquals(active.Root, root)) root.ActiveContent = null;

        // A typed group around another group of the same type adds no split.
        foreach (ILayoutElement node in new List<ILayoutElement>(root.Descendents()))
        {
            if (node is LayoutAnchorablePaneGroup tools)
                while (tools.ChildrenCount == 1 && tools.Children[0] is LayoutAnchorablePaneGroup child) Collapse(tools, child);
            if (node is LayoutDocumentPaneGroup documents)
                while (documents.ChildrenCount == 1 && documents.Children[0] is LayoutDocumentPaneGroup child) Collapse(documents, child);
        }
    }

    private static void Collapse(ILayoutOrientableGroup parent, ILayoutOrientableGroup child)
    {
        parent.Orientation = child.Orientation;
        List<ILayoutElement> contents = new(child.Children);
        parent.RemoveChild(child);
        foreach (ILayoutElement content in contents) parent.InsertChildAt(parent.ChildrenCount, content);
    }
}
