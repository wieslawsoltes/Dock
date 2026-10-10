using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

internal static class LayoutSourcePlacement
{
    internal static void DockAnchorable(LayoutRoot root, LayoutAnchorable tool, ILayoutUpdateStrategy? strategy)
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
        if (strategy?.BeforeInsertAnchorable(root, tool, destination) != true)
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
        strategy?.AfterInsertAnchorable(root, tool);
    }

    internal static void InsertDocument(LayoutRoot root, LayoutDocument document, ILayoutUpdateStrategy? strategy)
    {
        LayoutDocumentPane? destination = root.LastFocusedDocument?.Parent as LayoutDocumentPane;
        if (destination?.Root != root) destination = null;
        if (destination is null)
        {
            foreach (ILayoutElement element in root.Descendents())
                if (element is LayoutDocumentPane pane) { destination = pane; break; }
        }
        if (strategy?.BeforeInsertDocument(root, document, destination) != true)
        {
            if (destination is null) throw new InvalidOperationException("The layout must contain a document pane to host source documents.");
            destination.Children.Add(document);
        }
        strategy?.AfterInsertDocument(root, document);
    }

    internal static void InsertAnchorable(LayoutRoot root, LayoutAnchorable tool, ILayoutUpdateStrategy? strategy)
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
        if (strategy?.BeforeInsertAnchorable(root, tool, destination) != true)
        {
            if (destination is null)
            {
                LayoutPanel original = root.RootPanel;
                LayoutPanel outer = new() { Orientation = Orientation.Horizontal };
                root.RootPanel = outer;
                outer.Children.Add(original);
                destination = new LayoutAnchorablePane { DockWidth = new GridLength(200) };
                outer.Children.Add(destination);
            }
            destination.Children.Add(tool);
        }
        strategy?.AfterInsertAnchorable(root, tool);
    }
}
