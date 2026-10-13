using System;
using System.Collections.Generic;
using Orientation = Avalonia.Layout.Orientation;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Default actions operate on the public panes, whose collections own the native
// Dock models. Direct execution is separate from command availability.
internal static class LayoutItemOperations
{
    internal static LayoutContent RequireManagedContent(LayoutItem item)
    {
        LayoutContent content = item.LayoutElement!;
        if (content.Root!.Manager is null) throw new NullReferenceException();
        return content;
    }

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
        DockingManager manager = item.LayoutElement!.Root!.Manager!;
        List<LayoutContent> contents = new();
        foreach (ILayoutElement element in manager.Layout.Descendents())
            if (element is LayoutContent content && IsDocument(content) &&
                (!excludeCurrent || !ReferenceEquals(content, item.LayoutElement))) contents.Add(content);
        foreach (LayoutContent content in contents)
        {
            // Resolve each container after preceding callbacks, which may
            // themselves close documents or replace the manager's layout.
            if (!content.CanClose) continue;
            LayoutItem container = manager.GetLayoutItemFromModel(content)!;
            if (container.CloseCommand is { } command)
            {
                if (command.CanExecute(null)) command.Execute(null);
            }
            else content.Close();
        }
    }

    public static bool CanSplit(LayoutItem item, Orientation orientation)
    {
        if (!item.IsAttached || item.LayoutElement is not { } content || content is LayoutDocument { CanMove: false }) return false;
        LayoutDocumentPaneGroup? group = content.FindParent<LayoutDocumentPaneGroup>();
        // Native command sources query during parent/root notifications. The
        // registered manager is available before the root finishes attachment.
        return (group is null || group.ChildrenCount == 1 || item.Manager!.AllowMixedOrientation || group.Orientation == orientation) &&
            content.Parent is LayoutDocumentPane { ChildrenCount: > 1 };
    }

    public static void Split(LayoutItem item, Orientation orientation)
    {
        LayoutContent content = item.LayoutElement!;
        LayoutDocumentPaneGroup? group = content.FindParent<LayoutDocumentPaneGroup>();
        LayoutDocumentPane? pane = content.Parent as LayoutDocumentPane;
        if (group is null)
        {
            ILayoutContainer parent = pane!.Parent!;
            group = new LayoutDocumentPaneGroup { Orientation = orientation };
            parent.ReplaceChild(pane, group);
            group.Children.Add(pane);
        }
        group.Orientation = orientation;
        int insertionIndex = group.Children.IndexOf(pane!) + 1;
        // Moving into the new pane precedes its insertion notification. These
        // collections also transfer native ownership and initialize the factory.
        LayoutDocumentPane destination = new(content);
        group.Children.Insert(insertionIndex, destination);
        ActivateAndCollect(content);
    }

    public static bool CanMove(LayoutItem item, int direction) => AdjacentPane(item, direction) is not null;

    public static void Move(LayoutItem item, int direction)
    {
        LayoutContent content = item.LayoutElement!;
        LayoutDocumentPaneGroup group = content.FindParent<LayoutDocumentPaneGroup>()!;
        int index = group.Children.IndexOf((content.Parent as LayoutDocumentPane)!) + direction;
        LayoutDocumentPane destination = (group.Children[index] as LayoutDocumentPane)!;
        destination.Children.Insert(0, content);
        ActivateAndCollect(content);
    }

    private static LayoutDocumentPane? AdjacentPane(LayoutItem item, int direction)
    {
        if (!item.IsAttached || item.LayoutElement?.Parent is not LayoutDocumentPane pane ||
            item.LayoutElement.FindParent<LayoutDocumentPaneGroup>() is not { ChildrenCount: > 1 } group) return null;
        int index = group.Children.IndexOf(pane) + direction;
        return index >= 0 && index < group.Children.Count ? group.Children[index] as LayoutDocumentPane : null;
    }

    private static void ActivateAndCollect(LayoutContent content)
    {
        content.IsSelected = true;
        // A public collection transfer is passive. Complete native focus once
        // the destination has been published, as a factory transfer would do.
        if (content.DockModel.Owner is IDock pane && content.DockModel.Factory is { } factory)
            factory.SetFocusedDockable(pane, content.DockModel);
        content.IsActive = true;
        content.Root!.CollectGarbage();
    }

    private static bool IsDocument(LayoutContent content) =>
        content.Parent is LayoutDocumentPane or LayoutDocumentFloatingWindow;
}
