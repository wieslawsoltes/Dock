using System;
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
        ILayoutContainer? previous = content.PreviousContainer;
        if (previous is null || (floatingOnly && previous.FindParent<LayoutFloatingWindow>() is null)) return false;

        ILayoutContainer? current = content.Parent;
        // Float's return path requires a pane, whereas Dock also accepts a
        // floating document's single-child container or the hidden collection.
        int currentIndex = floatingOnly
            ? ((current as ILayoutPane) as ILayoutGroup)!.IndexOfChild(content)
            : (current as ILayoutGroup)?.IndexOfChild(content) ?? -1;
        ILayoutGroup group = (previous as ILayoutGroup)!;
        group.InsertChildAt(Math.Min(content.PreviousContainerIndex, group.ChildrenCount), content);

        // Public collection edits preserve the native model and publish the
        // requested insertion index immediately, including moves across roots.
        _removedFrom = null;
        content.PreviousContainer = currentIndex >= 0 ? current : null;
        content.PreviousContainerIndex = currentIndex >= 0 ? currentIndex : 0;
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
