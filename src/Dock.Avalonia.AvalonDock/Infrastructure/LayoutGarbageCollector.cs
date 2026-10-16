using System.Collections.Generic;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutGarbageCollector
{
    internal static void Collect(LayoutRoot root)
    {
        while (true)
        {
            ClearDisconnectedReferences(root);
            if (!RemoveEmptyPane(root)) break;
            RemoveEmptyContainer<LayoutAnchorablePaneGroup>(root, includeInitial: false);
            RemoveEmptyContainer<LayoutPanel>(root);
            RemoveEmptyContainer<LayoutFloatingWindow>(root);
            RemoveEmptyContainer<LayoutAnchorGroup>(root);
        }
        // Each family has its own phase. Rescan after a collapse because
        // application callbacks can add another candidate anywhere in the root.
        while (CollapseNext(root, documents: false)) { }
        while (CollapseNext(root, documents: true)) { }
    }

    private static void ClearDisconnectedReferences(LayoutRoot root)
    {
        foreach (ILayoutElement node in root.Descendents())
        {
            if (node is LayoutContent content && content.PreviousContainer is { } previous && previous.Parent?.Root != root)
                content.PreviousContainer = null;
            if (node is LayoutAnchorGroup group && group.PreviousContainer is { } groupPrevious && groupPrevious.Parent?.Root != root)
                group.PreviousContainer = null;
        }
        foreach (ILayoutElement node in root.Descendents())
            if (node is LayoutContent content && content.InitialContainer is { } initial && initial.Parent?.Root != root)
                content.InitialContainer = null;
    }

    private static bool RemoveEmptyPane(LayoutRoot root)
    {
        List<ILayoutPane> emptyPanes = new();
        foreach (ILayoutElement node in root.Descendents())
            if (node is ILayoutPane { ChildrenCount: 0 } pane) emptyPanes.Add(pane);
        foreach (ILayoutPane pane in emptyPanes)
        {
            foreach (ILayoutElement node in root.Descendents())
                if (node is LayoutContent content && ReferenceEquals(content.PreviousContainer, pane) &&
                    !content.IsFloating && content is not LayoutAnchorable { IsVisible: false })
                {
                    content.PreviousContainer = null;
                    content.PreviousContainerIndex = -1;
                }
            if (pane is LayoutDocumentPane document && !HasOtherDocumentPane(root, document)) continue;
            if (IsReferenced(root, pane)) continue;
            return Remove(pane);
        }
        return false;
    }

    private static bool HasOtherDocumentPane(LayoutRoot root, LayoutDocumentPane pane)
    {
        foreach (ILayoutElement node in root.Descendents())
            if (node is LayoutDocumentPane document && !ReferenceEquals(document, pane)) return true;
        return false;
    }

    private static void RemoveEmptyContainer<T>(LayoutRoot root, bool includeInitial = true) where T : ILayoutContainer
    {
        foreach (ILayoutElement node in root.Descendents())
        {
            if (node is not T { ChildrenCount: 0 } empty || IsReferenced(root, empty, includeInitial)) continue;
            // The document host owns deferred removal, including cancellation.
            if (empty is LayoutDocumentFloatingWindow { DockWindow.Host: Controls.LayoutFloatingWindowControl { HasClosed: false } }) continue;
            Remove(empty);
            return;
        }
    }

    private static bool IsReferenced(LayoutRoot root, ILayoutContainer container, bool includeInitial = true)
    {
        foreach (ILayoutElement node in root.Descendents())
        {
            if (node is LayoutContent content && (ReferenceEquals(content.PreviousContainer, container) ||
                includeInitial && ReferenceEquals(content.InitialContainer, container))) return true;
            if (node is LayoutAnchorGroup group && ReferenceEquals(group.PreviousContainer, container)) return true;
        }
        return false;
    }

    private static bool Remove(ILayoutContainer container)
    {
        ILayoutContainer parent = container.Parent!;
        parent.RemoveChild(container);
        // Floating children can retain their public Parent after removal.
        // Membership detects progress without looping on a native close veto.
        foreach (ILayoutElement child in parent.Children)
            if (ReferenceEquals(child, container)) return false;
        return true;
    }

    private static bool CollapseNext(LayoutRoot root, bool documents)
    {
        foreach (ILayoutElement node in root.Descendents())
        {
            if (!documents && node is LayoutAnchorablePaneGroup { ChildrenCount: 1 } tools && tools.Children[0] is LayoutAnchorablePaneGroup toolChild)
            {
                Collapse(tools, toolChild);
                return true;
            }
            if (documents && node is LayoutDocumentPaneGroup { ChildrenCount: 1 } pane && pane.Children[0] is LayoutDocumentPaneGroup documentChild)
            {
                Collapse(pane, documentChild);
                return true;
            }
        }
        return false;
    }

    private static void Collapse(ILayoutOrientableGroup parent, ILayoutOrientableGroup child)
    {
        parent.Orientation = child.Orientation;
        parent.RemoveChild(child);
        // Removal and insertion callbacks can change the remaining children.
        // Move the live first child on each iteration instead of a snapshot.
        while (child.ChildrenCount > 0)
            foreach (ILayoutElement content in child.Children)
            {
                parent.InsertChildAt(parent.ChildrenCount, content);
                break;
            }
    }
}
