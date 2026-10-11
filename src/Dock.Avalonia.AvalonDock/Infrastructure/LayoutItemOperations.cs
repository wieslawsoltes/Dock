using System.Collections.Generic;
using Orientation = Avalonia.Layout.Orientation;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Translates document-group commands to the same native models and factory
// used by dragging. There is no separate container-owned layout tree.
internal static class LayoutItemOperations
{
    public static bool HasDocuments(LayoutItem item, bool excludeCurrent)
    {
        if (!item.IsAttached || item.Manager is not { } manager) return false;
        foreach (ILayoutElement element in manager.Layout.Descendents())
            if (element is LayoutContent content && IsDocument(content) &&
                (!excludeCurrent || !ReferenceEquals(content, item.LayoutElement))) return true;
        return false;
    }

    public static void CloseDocuments(LayoutItem item, bool excludeCurrent)
    {
        if (item.Manager is not { } manager) return;
        List<LayoutContent> contents = new();
        foreach (ILayoutElement element in manager.Layout.Descendents())
            if (element is LayoutContent content && IsDocument(content) &&
                (!excludeCurrent || !ReferenceEquals(content, item.LayoutElement))) contents.Add(content);
        foreach (LayoutContent content in contents)
        {
            // Resolve each container after preceding callbacks, which may
            // themselves close documents or replace the manager's layout.
            if (!LayoutCommandCapabilities.CanClose(content) || manager.GetLayoutItemFromModel(content) is not { } container) continue;
            if (container.CloseCommand is { } command)
            {
                if (command.CanExecute(null)) command.Execute(null);
            }
            else content.Close();
        }
    }

    public static bool CanSplit(LayoutItem item, Orientation orientation)
    {
        if (!item.IsAttached || item.LayoutElement is LayoutDocument { CanMove: false } ||
            item.LayoutElement?.Parent is not LayoutDocumentPane { ChildrenCount: > 1 } pane) return false;
        return pane.Parent is not LayoutDocumentPaneGroup group || group.ChildrenCount == 1 ||
            group.Orientation == orientation || item.Manager!.AllowMixedOrientation;
    }

    public static void Split(LayoutItem item, Orientation orientation)
    {
        if (!CanSplit(item, orientation) || item.LayoutElement is not { } content ||
            content.Parent is not LayoutDocumentPane pane || pane.Parent is not { } parent) return;
        LayoutDocumentPaneGroup group;
        if (parent is LayoutDocumentPaneGroup existing) group = existing;
        else
        {
            group = new LayoutDocumentPaneGroup();
            parent.ReplaceChild(pane, group);
            group.Children.Add(pane);
        }
        group.Orientation = orientation;
        LayoutDocumentPane destination = new();
        group.Children.Insert(group.Children.IndexOf(pane) + 1, destination);
        Move(content, destination);
    }

    public static bool CanMove(LayoutItem item, int direction) => AdjacentPane(item, direction) is not null;

    public static void Move(LayoutItem item, int direction)
    {
        if (AdjacentPane(item, direction) is { } destination && item.LayoutElement is { } content)
            Move(content, destination);
    }

    private static LayoutDocumentPane? AdjacentPane(LayoutItem item, int direction)
    {
        if (!item.IsAttached || item.LayoutElement is LayoutDocument { CanMove: false } ||
            item.LayoutElement?.Parent is not LayoutDocumentPane pane ||
            pane.Parent is not LayoutDocumentPaneGroup group) return null;
        int index = group.Children.IndexOf(pane) + direction;
        return index >= 0 && index < group.Children.Count ? group.Children[index] as LayoutDocumentPane : null;
    }

    private static void Move(LayoutContent content, LayoutDocumentPane destination)
    {
        if (content.DockModel.Owner is not IDock source || content.DockModel.Factory is not { } factory) return;
        // Upstream tab-group commands manipulate panes without docking events.
        using IDockOperationScope? scope = (content.Root as LayoutRoot)?.Operations.Begin(content.DockModel, LayoutOperation.Silent);
        factory.MoveDockable(source, (IDock)destination.DockModel, content.DockModel, null);
        int index = destination.Children.IndexOf(content);
        if (index > 0) destination.MoveChild(index, 0);
        content.IsSelected = true;
        content.IsActive = true;
        content.Root?.CollectGarbage();
    }

    private static bool IsDocument(LayoutContent content) =>
        content.Parent is LayoutDocumentPane or LayoutDocumentFloatingWindow;
}
