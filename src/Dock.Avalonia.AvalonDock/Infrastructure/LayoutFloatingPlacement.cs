using System;
using System.Collections.Generic;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal interface ILayoutFloatingPlacement
{
    void FloatAll(IDockable dockable, Action operation);
    void FloatContainer(IDockable dockable, Action operation);
}

// A native batch move has no per-content completion event. Capture its public
// return positions before Dock removes the first tab, then commit them once
// the native window owns the moved contents.
internal sealed class LayoutFloatingPlacement(LayoutRoot root) : ILayoutFloatingPlacement
{
    public void FloatAll(IDockable dockable, Action operation)
    {
        if (dockable.Owner is not ToolDock model || root.Context.Get(model) is not LayoutAnchorablePane pane)
        {
            operation();
            return;
        }
        List<ReturnPosition> positions = new(pane.ChildrenCount);
        for (int i = 0; i < pane.ChildrenCount; i++)
        {
            if (!pane.Children[i].CanFloat) return;
            positions.Add(new(pane.Children[i], pane, i));
        }
        bool remember = !LayoutDockingHistory.IsFloatingContainer(pane);
        LayoutContent? selected = pane.SelectedContent;
        try { operation(); }
        finally
        {
            if (Complete(positions, remember, selected))
            {
                if (positions[0].Content.Parent is ILayoutPositionableGroup target) CopyPosition(pane, target);
                root.CollectGarbage();
            }
        }
    }

    public void FloatContainer(IDockable dockable, Action operation)
    {
        if (dockable is not ToolDock and not ProportionalDock ||
            root.Context.Get(dockable) is not ILayoutAnchorablePane source || source is not ILayoutGroup sourceGroup)
        {
            operation();
            return;
        }
        foreach (ILayoutElement element in source.Descendents())
            if (element is LayoutContent { CanFloat: false }) return;

        bool remember = !LayoutDockingHistory.IsFloatingContainer(source);
        ILayoutGroup? parent = source.Parent as ILayoutGroup;
        if (remember && parent is null)
        {
            operation();
            return;
        }
        List<ReturnPosition> positions = new();
        ILayoutAnchorablePane? destination = null;
        if (remember) destination = CloneReturnTree(source, positions);
        else
            foreach (ILayoutElement element in source.Descendents())
                if (element is LayoutAnchorable tool && tool.Parent is LayoutAnchorablePane previous)
                    positions.Add(new(tool, previous, previous.IndexOfChild(tool)));
        LayoutContent? selected = source is LayoutAnchorablePane pane ? pane.SelectedContent : root.ActiveContent;
        if (selected is null || !positions.Exists(item => ReferenceEquals(item.Content, selected)))
            selected = FindSelected(positions);

        if (destination is not null) parent!.InsertChildAt(parent.IndexOfChild(sourceGroup), destination);
        try { operation(); }
        finally
        {
            if (Complete(positions, remember, selected)) root.CollectGarbage();
            else destination?.Parent?.RemoveChild(destination);
        }
    }

    private bool Complete(List<ReturnPosition> positions, bool remember, LayoutContent? selected)
    {
        bool moved = false;
        foreach (ReturnPosition position in positions)
        {
            if (!position.Content.IsFloating || !ReferenceEquals(position.Content.Root, root)) continue;
            moved = true;
            if (remember) position.Content.DockingHistory.Remember(position.Pane, position.Index);
        }
        if (moved && selected is { IsFloating: true })
        {
            selected.IsSelected = true;
            selected.IsActive = true;
        }
        return moved;
    }

    private static LayoutContent? FindSelected(List<ReturnPosition> positions)
    {
        foreach (ReturnPosition position in positions) if (position.Content.IsSelected) return position.Content;
        return positions.Count > 0 ? positions[0].Content : null;
    }

    private static ILayoutAnchorablePane CloneReturnTree(ILayoutAnchorablePane source, List<ReturnPosition> positions)
    {
        if (source is LayoutAnchorablePane pane)
        {
            LayoutAnchorablePane result = new() { Name = pane.Name };
            CopyPosition(pane, result);
            ((ToolDock)result.DockModel).Alignment = ((ToolDock)pane.DockModel).Alignment;
            result.DockModel.Proportion = pane.DockModel.Proportion;
            for (int i = 0; i < pane.Children.Count; i++) positions.Add(new(pane.Children[i], result, i));
            return result;
        }
        LayoutAnchorablePaneGroup group = (LayoutAnchorablePaneGroup)source;
        LayoutAnchorablePaneGroup copy = new() { Orientation = group.Orientation };
        CopyPosition(group, copy);
        copy.DockModel.Proportion = group.DockModel.Proportion;
        foreach (ILayoutAnchorablePane child in group.Children) copy.Children.Add(CloneReturnTree(child, positions));
        return copy;
    }

    private static void CopyPosition(ILayoutPositionableGroup source, ILayoutPositionableGroup target)
    {
        target.DockWidth = source.DockWidth;
        target.DockHeight = source.DockHeight;
        target.DockMinWidth = source.DockMinWidth;
        target.DockMinHeight = source.DockMinHeight;
        target.AllowDuplicateContent = source.AllowDuplicateContent;
        target.CanRepositionItems = source.CanRepositionItems;
    }

    private readonly record struct ReturnPosition(LayoutContent Content, LayoutAnchorablePane Pane, int Index);
}
