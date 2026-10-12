using System;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Native Dock moves remove a child before reporting the completed operation.
// Keep the original position until that notification, without retaining the
// old tree when a caller only removes and keeps the content.
internal sealed class LayoutDockingHistory(LayoutContent content)
{
    private WeakReference<ILayoutContainer>? _removedFrom;
    private int _removedIndex;
    private bool _wasFloating;

    internal void CaptureRemoval(ILayoutContainer container, int index)
    {
        _removedFrom = new(container);
        _removedIndex = index;
        _wasFloating = IsFloatingContainer(container);
    }

    internal void CompleteMove()
    {
        WeakReference<ILayoutContainer>? location = _removedFrom;
        _removedFrom = null;
        if (location is null || _wasFloating == content.IsFloating) return;
        if (location.TryGetTarget(out ILayoutContainer? container) && _removedIndex >= 0 &&
            ReferenceEquals(container.Root, content.Root))
            Remember(container, _removedIndex);
        else Remember(null, -1);
    }

    internal void CompleteHide()
    {
        WeakReference<ILayoutContainer>? location = _removedFrom;
        _removedFrom = null;
        if (location?.TryGetTarget(out ILayoutContainer? container) == true && _removedIndex >= 0 &&
            ReferenceEquals(container.Root, content.Root))
            Remember(container, _removedIndex);
    }

    internal bool RestorePrevious(bool floatingOnly = false)
    {
        if (content.Root is not LayoutRoot root || content.PreviousContainer is not ILayoutGroup group ||
            group is not LayoutElement { DockModel: IDock target } || !ReferenceEquals(group.Root, root) ||
            (floatingOnly && !IsFloatingContainer(group)) || content.DockModel.Owner is not IDock source ||
            content.DockModel.Factory is not { } factory || ReferenceEquals(source, target)) return false;

        int index = content.PreviousContainerIndex;
        // Upstream Float reuses a previous floating pane without creating a
        // window or raising the manager's float/dock notifications.
        using IDockOperationScope? silent = floatingOnly ? root.Operations.Begin(content.DockModel, LayoutOperation.Silent) : null;
        factory.MoveDockable(source, target, content.DockModel, null);
        if (!ReferenceEquals(content.Parent, group)) return false;
        if (group is ILayoutPane pane)
        {
            int current = group.IndexOfChild(content);
            int desired = index < 0 ? group.ChildrenCount - 1 : Math.Min(index, group.ChildrenCount - 1);
            if (current != desired) pane.MoveChild(current, desired);
        }
        return true;
    }

    internal void Remember(ILayoutContainer? container, int index)
    {
        _removedFrom = null;
        content.PreviousContainer = container;
        content.PreviousContainerIndex = index;
        content.PreviousContainerId = (container as LayoutElement)?.DockModel.Id;
    }

    internal static bool IsFloatingContainer(ILayoutElement element) =>
        element is LayoutFloatingWindow || element.FindParent<LayoutFloatingWindow>() is not null;
}
