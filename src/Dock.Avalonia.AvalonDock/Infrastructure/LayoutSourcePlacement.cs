using System;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutSourcePlacement
{
    internal static void DockAnchorable(LayoutRoot root, LayoutAnchorable tool)
    {
        ILayoutGroup? destination = !ReferenceEquals(root.ActiveContent, tool)
            ? root.ActiveContent?.Parent as LayoutAnchorablePane : null;
        LayoutAnchorablePane? firstToolPane = null;
        LayoutDocumentPane? firstDocumentPane = null;
        if (destination is null)
        {
            foreach (ILayoutElement element in root.Descendents())
            {
                if (element is LayoutDocumentPane documents) firstDocumentPane ??= documents;
                if (element is not LayoutAnchorablePane pane || pane.IsHostedInFloatingWindow) continue;
                firstToolPane ??= pane;
                if (pane.GetSide() == AnchorSide.Right) { destination = pane; break; }
            }
            destination ??= (ILayoutGroup?)firstToolPane ?? firstDocumentPane;
        }
        if (root.Manager!.LayoutUpdateStrategy?.BeforeInsertAnchorable(root, tool, destination) != true)
        {
            if (destination is null)
            {
                LayoutPanel original = root.RootPanel;
                LayoutPanel outer = new() { Orientation = Orientation.Horizontal };
                root.RootPanel = outer;
                outer.Children.Add(original);
                LayoutAnchorablePane pane = new() { DockWidth = new GridLength(200) };
                outer.Children.Add(pane);
                destination = pane;
            }
            destination.InsertChildAt(destination.ChildrenCount, tool);
        }
        root.Manager!.LayoutUpdateStrategy?.AfterInsertAnchorable(root, tool);
    }

    internal static void DockDocument(LayoutRoot root, LayoutDocument document)
    {
        LayoutDocumentPane? destination = FindDocumentDestination(root, document);
        if (root.Manager!.LayoutUpdateStrategy?.BeforeInsertDocument(root, document, destination) != true)
        {
            if (destination is null)
                throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");
            destination.Children.Add(document);
        }
        root.Manager!.LayoutUpdateStrategy?.AfterInsertDocument(root, document);
    }

    internal static LayoutDocumentPane? FindDocumentDestination(LayoutRoot root)
        => FindDocumentDestination(root, null);

    private static LayoutDocumentPane? FindDocumentDestination(LayoutRoot root, LayoutDocument? dockingDocument)
    {
        LayoutDocumentPane? destination = !ReferenceEquals(root.LastFocusedDocument, dockingDocument)
            ? root.LastFocusedDocument?.Parent as LayoutDocumentPane : null;
        if (destination?.Root != root) destination = null;
        if (destination is null)
        {
            foreach (ILayoutElement element in root.Descendents())
                if (element is LayoutDocumentPane pane) { destination = pane; break; }
        }
        return destination;
    }

    internal static ILayoutContainer? InsertDocument(LayoutRoot? fixedLayout, LayoutDocument document,
        ILayoutContainer? destination, DockingManager? sourceManager)
    {
        LayoutRoot root = fixedLayout ?? sourceManager!.Layout;
        if ((sourceManager ?? root.Manager)?.LayoutUpdateStrategy?.BeforeInsertDocument(root, document, destination) != true)
        {
            if (destination is null) throw new InvalidOperationException("The layout must contain a document pane to host source documents.");
            ((LayoutDocumentPane)destination).Children.Add(document);
        }
        (sourceManager ?? root.Manager)?.LayoutUpdateStrategy?.AfterInsertDocument(fixedLayout ?? sourceManager!.Layout, document);
        return destination;
    }

    internal static LayoutAnchorablePane? FindAnchorableDestination(LayoutRoot root)
    {
        LayoutAnchorablePane? destination = root.ActiveContent?.Parent as LayoutAnchorablePane;
        LayoutAnchorablePane? first = null;
        if (destination is null)
        {
            foreach (ILayoutElement element in root.Descendents())
            {
                if (element is not LayoutAnchorablePane pane) continue;
                first ??= pane;
                if (!pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right) { destination = pane; break; }
            }
            destination ??= first;
        }
        return destination;
    }

    internal static ILayoutContainer? InsertAnchorable(LayoutRoot? fixedLayout, LayoutAnchorable tool,
        ILayoutContainer? destination, DockingManager? sourceManager)
    {
        LayoutRoot root = fixedLayout ?? sourceManager!.Layout;
        if ((sourceManager ?? root.Manager)?.LayoutUpdateStrategy?.BeforeInsertAnchorable(root, tool, destination) != true)
        {
            if (destination is null)
            {
                root = fixedLayout ?? sourceManager!.Layout;
                LayoutPanel original = root.RootPanel;
                LayoutPanel outer = new() { Orientation = Orientation.Horizontal };
                root.RootPanel = outer;
                outer.Children.Add(original);
                LayoutAnchorablePane pane = new() { DockWidth = new GridLength(200) };
                outer.Children.Add(pane);
                destination = pane;
            }
            ((LayoutAnchorablePane)destination).Children.Add(tool);
        }
        (sourceManager ?? root.Manager)?.LayoutUpdateStrategy?.AfterInsertAnchorable(fixedLayout ?? sourceManager!.Layout, tool);
        return destination;
    }
}
