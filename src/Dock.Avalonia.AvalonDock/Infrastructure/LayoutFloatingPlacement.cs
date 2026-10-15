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
        if (dockable.Owner is not (ToolDock or DocumentDock) ||
            root.Context.Get(dockable.Owner) is not ILayoutGroup pane || pane is not ILayoutContentSelector selector)
        {
            operation();
            return;
        }
        List<ReturnPosition> positions = new(pane.ChildrenCount);
        foreach (ILayoutElement child in pane.Children)
        {
            if (child is not LayoutContent content) continue;
            if (!content.CanFloat) return;
            positions.Add(new(content, pane, pane.IndexOfChild(content)));
        }
        bool remember = !LayoutDockingHistory.IsFloatingContainer(pane);
        LayoutContent? selected = selector.SelectedContent;
        try { operation(); }
        finally
        {
            if (Complete(positions, remember, selected))
            {
                if (pane is ILayoutPositionableGroup source && positions[0].Content.Parent is ILayoutPositionableGroup target)
                    CopyPosition(source, target);
                root.CollectGarbage();
            }
        }
    }

    public void FloatContainer(IDockable dockable, Action operation)
    {
        if (dockable is not ToolDock and not DocumentDock and not ProportionalDock ||
            root.Context.Get(dockable) is not ILayoutPanelElement source || source is not ILayoutGroup sourceGroup)
        {
            operation();
            return;
        }
        foreach (ILayoutElement element in source.Descendents())
            if (element is LayoutContent { CanFloat: false }) return;

        bool remember = !LayoutDockingHistory.IsFloatingContainer(source);
        ILayoutGroup? parent = source.Parent as ILayoutGroup;
        bool entireRoot = source is LayoutPanel && ReferenceEquals(source.Parent, root);
        if (remember && parent is null && !entireRoot)
        {
            operation();
            return;
        }
        List<ReturnPosition> positions = new();
        ILayoutPanelElement? destination = null;
        if (remember) destination = CloneReturnTree(source, positions);
        else
            foreach (ILayoutElement element in source.Descendents())
                if (element is LayoutContent content && content.Parent is ILayoutGroup previous)
                    positions.Add(new(content, previous, previous.IndexOfChild(content)));
        LayoutContent? selected = source is ILayoutContentSelector pane ? pane.SelectedContent : root.ActiveContent;
        if (selected is null || !positions.Exists(item => ReferenceEquals(item.Content, selected)))
            selected = FindSelected(positions);

        if (entireRoot)
        {
            // Keep a valid public root throughout native removal notifications.
            // SplitToWindow resolves the original root, then removes the source
            // from this replacement panel using its current native owner.
            LayoutPanel replacement = new();
            root.RootPanel = replacement;
            replacement.Children.Add(destination!);
            replacement.Children.Add(source);
        }
        else if (destination is not null) parent!.InsertChildAt(parent.IndexOfChild(sourceGroup), destination);
        try { operation(); }
        finally
        {
            if (Complete(positions, remember, selected)) root.CollectGarbage();
            else if (entireRoot && source.Parent is LayoutPanel) root.RootPanel = (LayoutPanel)source;
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

    private static ILayoutPanelElement CloneReturnTree(ILayoutPanelElement source, List<ReturnPosition> positions)
    {
        ILayoutPanelElement copy;
        if (source is LayoutAnchorablePane tools)
        {
            LayoutAnchorablePane pane = new() { Name = tools.Name };
            ((ToolDock)pane.DockModel).Alignment = ((ToolDock)tools.DockModel).Alignment;
            copy = pane;
        }
        else copy = source switch
        {
            LayoutDocumentPane pane => new LayoutDocumentPane { ShowHeader = pane.ShowHeader },
            LayoutDocumentPaneGroup group => new LayoutDocumentPaneGroup { Orientation = group.Orientation },
            LayoutAnchorablePaneGroup group => new LayoutAnchorablePaneGroup { Orientation = group.Orientation },
            LayoutPanel panel => new LayoutPanel { Orientation = panel.Orientation },
            _ => throw new NotSupportedException("The floating group has no return-layout projection.")
        };
        CopyPosition((ILayoutPositionableGroup)source, (ILayoutPositionableGroup)copy);
        ((LayoutElement)copy).DockModel.Proportion = ((LayoutElement)source).DockModel.Proportion;
        ILayoutGroup target = (ILayoutGroup)copy;
        foreach (ILayoutElement child in ((ILayoutGroup)source).Children)
        {
            if (child is LayoutContent content)
                positions.Add(new(content, target, ((ILayoutGroup)source).IndexOfChild(content)));
            else target.InsertChildAt(target.ChildrenCount, CloneReturnTree((ILayoutPanelElement)child, positions));
        }
        return copy;
    }

    internal static void CopyPosition(ILayoutPositionableGroup source, ILayoutPositionableGroup target)
    {
        target.DockWidth = source.DockWidth;
        target.DockHeight = source.DockHeight;
        target.DockMinWidth = source.DockMinWidth;
        target.DockMinHeight = source.DockMinHeight;
        target.AllowDuplicateContent = source.AllowDuplicateContent;
        target.CanRepositionItems = source.CanRepositionItems;
    }

    private readonly record struct ReturnPosition(LayoutContent Content, ILayoutGroup Pane, int Index);
}
