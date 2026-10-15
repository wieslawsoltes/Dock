using System;
using System.Collections.Generic;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;
using Orientation = Avalonia.Layout.Orientation;

namespace AvalonDock.Infrastructure;

// Public anchor groups retain AvalonDock return locations over Dock's flat
// pinned collections. Native pinning still owns movement, previews, and events.
internal static class LayoutAutoHideOperations
{
    internal static void Toggle(LayoutAnchorable tool)
    {
        if (tool.Root is not LayoutRoot root) return;
        if (tool.Parent is LayoutAnchorGroup group && group.Parent is LayoutAnchorSide side)
        {
            LayoutAnchorablePane destination = group.PreviousContainer is LayoutAnchorablePane previous &&
                ReferenceEquals(previous.Root, root) ? previous : CreateReturnPane(root, tool, side);
            RedirectReturns(root, group, destination);
            ToggleNative(root, tool, destination);
            return;
        }
        if (tool.Parent is not LayoutAnchorablePane pane) return;
        AnchorSide edge = pane.GetSide();
        LayoutAnchorSide strip = (edge switch
        {
            AnchorSide.Left => root.LeftSide,
            AnchorSide.Top => root.TopSide,
            AnchorSide.Right => root.RightSide,
            _ => root.BottomSide
        })!;
        tool.RememberDockedLocation();
        ((IToolDock)pane.DockModel).Alignment = strip.DockAlignment;
        strip.Pin(tool, () => ToggleNative(root, tool, null));
    }

    private static void ToggleNative(LayoutRoot root, LayoutAnchorable tool, LayoutAnchorablePane? destination)
    {
        if (root.DockFactory is LayoutFactory factory)
            factory.ToggleAutoHideContent(tool.DockModel, (IRootDock)root.DockModel, destination?.DockModel as IToolDock);
        else root.DockFactory.PinDockable(tool.DockModel);
    }

    private static LayoutAnchorablePane CreateReturnPane(LayoutRoot root, LayoutAnchorable tool, LayoutAnchorSide side)
    {
        bool horizontal = side.Side is AnchorSide.Left or AnchorSide.Right;
        bool first = side.Side is AnchorSide.Left or AnchorSide.Top;
        Orientation orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical;
        LayoutAnchorablePane pane = new();
        ((IToolDock)pane.DockModel).Alignment = side.DockAlignment;
        LayoutPanel panel = root.RootPanel;
        if (panel.Orientation == orientation)
        {
            if (horizontal) pane.DockMinWidth = tool.AutoHideMinWidth;
            else pane.DockMinHeight = tool.AutoHideMinHeight;
        }
        else
        {
            LayoutPanel outer = new() { Orientation = orientation };
            root.RootPanel = outer;
            outer.Children.Add(panel);
            panel = outer;
        }
        panel.Children.Insert(first ? 0 : panel.Children.Count, pane);
        return pane;
    }

    private static void RedirectReturns(LayoutRoot root, LayoutAnchorGroup group, LayoutAnchorablePane destination)
    {
        foreach (ILayoutElement element in new List<ILayoutElement>(root.Descendents()))
        {
            if (element is LayoutContent content && ReferenceEquals(content.PreviousContainer, group))
            {
                content.PreviousContainer = destination;
                content.PreviousContainerId = destination.DockModel.Id;
                if (content is LayoutAnchorable { IsHidden: true }) content.DockModel.OriginalOwner = destination.DockModel;
            }
            else if (element is LayoutAnchorGroup other && ReferenceEquals(other.PreviousContainer, group))
            {
                other.PreviousContainer = destination;
                other.PreviousContainerId = destination.DockModel.Id;
            }
        }
    }
}
