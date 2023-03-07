using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void AddDockable(IDock dock, IDockable dockable)
    {
        InitDockable(dockable, dock);
        dock.VisibleDockables ??= CreateList<IDockable>();
        dock.VisibleDockables.Add(dockable);
        OnDockableAdded(dockable);
    }

    /// <inheritdoc/>
    public virtual void InsertDockable(IDock dock, IDockable dockable, int index)
    {
        if (index >= 0)
        {
            InitDockable(dockable, dock);
            dock.VisibleDockables ??= CreateList<IDockable>();
            dock.VisibleDockables.Insert(index, dockable);
            OnDockableAdded(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveDockable(IDockable dockable, bool collapse)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        var index = dock.VisibleDockables.IndexOf(dockable);
        if (index < 0)
        {
            return;
        }

        dock.VisibleDockables.Remove(dockable);
        OnDockableRemoved(dockable);

        var indexActiveDockable = index > 0 ? index - 1 : 0;
        if (dock.VisibleDockables.Count > 0)
        {
            var nextActiveDockable = dock.VisibleDockables[indexActiveDockable];
            dock.ActiveDockable = nextActiveDockable is not IProportionalDockSplitter ? nextActiveDockable : null;
        }
        else
        {
            dock.ActiveDockable = null;
        }

        if (dock.VisibleDockables.Count == 1)
        {
            var dockable0 = dock.VisibleDockables[0];
            if (dockable0 is IProportionalDockSplitter splitter0)
            {
                RemoveDockable(splitter0, false);
            }
        }

        if (dock.VisibleDockables.Count == 2)
        {
            var dockable0 = dock.VisibleDockables[0];
            var dockable1 = dock.VisibleDockables[1];
            if (dockable0 is IProportionalDockSplitter splitter0)
            {
                RemoveDockable(splitter0, false);
            }
            if (dockable1 is IProportionalDockSplitter splitter1)
            {
                RemoveDockable(splitter1, false);
            }
        }

        if (collapse)
        {
            CollapseDock(dock);
        }
    }

    /// <inheritdoc/>
    public virtual void MoveDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            dock.VisibleDockables.RemoveAt(sourceIndex);
            OnDockableRemoved(sourceDockable);
            dock.VisibleDockables.Insert(targetIndex, sourceDockable);
            OnDockableAdded(sourceDockable);
            OnDockableMoved(sourceDockable);
            dock.ActiveDockable = sourceDockable;
        }
    }

    /// <inheritdoc/>
    public virtual void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        if (targetDock.VisibleDockables is null)
        {
            targetDock.VisibleDockables = CreateList<IDockable>();
            if (targetDock.VisibleDockables is null)
            {
                return;
            }
        }

        var isSameOwner = sourceDock == targetDock;

        var targetIndex = 0;

        if (sourceDock.VisibleDockables is not null && targetDock.VisibleDockables is not null && targetDock.VisibleDockables.Count > 0)
        {
            if (isSameOwner)
            {
                var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);

                if (targetDockable is not null)
                {
                    targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);
                }
                else
                {
                    targetIndex = targetDock.VisibleDockables.Count - 1;
                }

                if (sourceIndex == targetIndex)
                {
                    return;
                }
            }
            else
            {
                if (targetDockable is not null)
                {
                    targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);
                    if (targetIndex >= 0)
                    {
                        targetIndex += 1;
                    }
                    else
                    {
                        targetIndex = targetDock.VisibleDockables.Count - 1;
                    }
                }
                else
                {
                    targetIndex = targetDock.VisibleDockables.Count - 1;
                }
            }
        }

        if (sourceDock.VisibleDockables is not null && targetDock.VisibleDockables is not null)
        {
            if (isSameOwner)
            {
                var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);
                if (sourceIndex < targetIndex)
                {
                    targetDock.VisibleDockables.Insert(targetIndex + 1, sourceDockable);
                    OnDockableAdded(sourceDockable);
                    targetDock.VisibleDockables.RemoveAt(sourceIndex);
                    OnDockableRemoved(sourceDockable);
                    OnDockableMoved(sourceDockable);
                }
                else
                {
                    var removeIndex = sourceIndex + 1;
                    if (targetDock.VisibleDockables.Count + 1 > removeIndex)
                    {
                        targetDock.VisibleDockables.Insert(targetIndex, sourceDockable);
                        OnDockableAdded(sourceDockable);
                        targetDock.VisibleDockables.RemoveAt(removeIndex);
                        OnDockableRemoved(sourceDockable);
                        OnDockableMoved(sourceDockable);
                    }
                }
            }
            else
            {
                RemoveDockable(sourceDockable, true);
                targetDock.VisibleDockables.Insert(targetIndex, sourceDockable);
                OnDockableAdded(sourceDockable);
                OnDockableMoved(sourceDockable);
                InitDockable(sourceDockable, targetDock);
                targetDock.ActiveDockable = sourceDockable;
            }
        }
    }

    /// <inheritdoc/>
    public virtual void SwapDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            var originalSourceDockable = dock.VisibleDockables[sourceIndex];
            var originalTargetDockable = dock.VisibleDockables[targetIndex];

            dock.VisibleDockables[targetIndex] = originalSourceDockable;
            OnDockableRemoved(originalTargetDockable);
            OnDockableAdded(originalSourceDockable);
            dock.VisibleDockables[sourceIndex] = originalTargetDockable;
            OnDockableAdded(originalTargetDockable);
            OnDockableSwapped(originalSourceDockable);
            OnDockableSwapped(originalTargetDockable);
            dock.ActiveDockable = originalTargetDockable;
        }
    }

    /// <inheritdoc/>
    public virtual void SwapDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable)
    {
        if (sourceDock.VisibleDockables is null || targetDock.VisibleDockables is null)
        {
            return;
        }

        var sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);
        var targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);

        if (sourceIndex >= 0 && targetIndex >= 0)
        {
            var originalSourceDockable = sourceDock.VisibleDockables[sourceIndex];
            var originalTargetDockable = targetDock.VisibleDockables[targetIndex];
            sourceDock.VisibleDockables[sourceIndex] = originalTargetDockable;
            targetDock.VisibleDockables[targetIndex] = originalSourceDockable;
                
            InitDockable(originalSourceDockable, targetDock);
            InitDockable(originalTargetDockable, sourceDock);

            OnDockableSwapped(originalTargetDockable);
            OnDockableSwapped(originalSourceDockable);

            sourceDock.ActiveDockable = originalTargetDockable;
            targetDock.ActiveDockable = originalSourceDockable;
        }
    }

    private bool IsDockablePinned(IDockable dockable, IRootDock rootDock)
    {
        if (rootDock.LeftPinnedDockables is not null)
        {
            if (rootDock.LeftPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.RightPinnedDockables is not null)
        {
            if (rootDock.RightPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.TopPinnedDockables is not null)
        {
            if (rootDock.TopPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        if (rootDock.BottomPinnedDockables is not null)
        {
            if (rootDock.BottomPinnedDockables.Contains(dockable))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public virtual void PinDockable(IDockable dockable)
    {
        switch (dockable.Owner)
        {
            case IToolDock toolDock:
            {
                var rootDock = FindRoot(dockable, _ => true);
                if (rootDock is null)
                {
                    return;
                }
                
                var isVisible = false;

                if (toolDock.VisibleDockables is not null)
                {
                    isVisible = toolDock.VisibleDockables.Contains(dockable);
                }

                var isPinned = IsDockablePinned(dockable, rootDock);

                var alignment = toolDock.Alignment;

                if (isVisible && !isPinned)
                {
                    // Pin dockable.

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            rootDock.LeftPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Right:
                        {
                            rootDock.RightPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Top:
                        {
                            rootDock.TopPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                        case Alignment.Bottom:
                        {
                            rootDock.BottomPinnedDockables ??= CreateList<IDockable>();
                            break;
                        }
                    }
                    
                    if (toolDock.VisibleDockables is not null)
                    {
                        toolDock.VisibleDockables.Remove(dockable);
                        OnDockableRemoved(dockable);
                    }

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            if (rootDock.LeftPinnedDockables is not null)
                            {
                                rootDock.LeftPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Right:
                        {
                            if (rootDock.RightPinnedDockables is not null)
                            {
                                rootDock.RightPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Top:
                        {
                            if (rootDock.TopPinnedDockables is not null)
                            {
                                rootDock.TopPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Bottom:
                        {
                            if (rootDock.BottomPinnedDockables is not null)
                            {
                                rootDock.BottomPinnedDockables.Add(dockable);
                                OnDockablePinned(dockable);
                            }

                            break;
                        }
                    }

                    // TODO: Handle ActiveDockable state.
                    // TODO: Handle IsExpanded property of IToolDock.
                    // TODO: Handle AutoHide property of IToolDock.
                }
                else if (!isVisible && isPinned)
                {
                    // Unpin dockable.

                    toolDock.VisibleDockables ??= CreateList<IDockable>();

                    switch (alignment)
                    {
                        case Alignment.Unset:
                        case Alignment.Left:
                        {
                            if (rootDock.LeftPinnedDockables is not null)
                            {
                                rootDock.LeftPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Right:
                        {
                            if (rootDock.RightPinnedDockables is not null)
                            {
                                rootDock.RightPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Top:
                        {
                            if (rootDock.TopPinnedDockables is not null)
                            {
                                rootDock.TopPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                        case Alignment.Bottom:
                        {
                            if (rootDock.BottomPinnedDockables is not null)
                            {
                                rootDock.BottomPinnedDockables.Remove(dockable);
                                OnDockableUnpinned(dockable);
                            }

                            break;
                        }
                    }

                    toolDock.VisibleDockables.Add(dockable);
                    OnDockableAdded(dockable);
                    
                    // TODO: Handle ActiveDockable state.
                    // TODO: Handle IsExpanded property of IToolDock.
                    // TODO: Handle AutoHide property of IToolDock.
                }
                else
                {
                    // TODO: Handle invalid state.
                }

                break;
            }
        }
    }

    /// <inheritdoc/>
    public virtual void FloatDockable(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock)
        {
            return;
        }

        dock.GetPointerScreenPosition(out var dockPointerScreenX, out var dockPointerScreenY);
        dockable.GetPointerScreenPosition(out var dockablePointerScreenX, out var dockablePointerScreenY);

        if (double.IsNaN(dockablePointerScreenX))
        {
            dockablePointerScreenX = dockPointerScreenX;
        }
        if (double.IsNaN(dockablePointerScreenY))
        {
            dockablePointerScreenY = dockPointerScreenY;
        }

        dock.GetVisibleBounds(out var ownerX, out var ownerY, out var ownerWidth, out var ownerHeight);
        dockable.GetVisibleBounds(out var dockableX, out var dockableY, out var dockableWidth, out var dockableHeight);

        if (double.IsNaN(dockablePointerScreenX))
        {
            dockablePointerScreenX = !double.IsNaN(dockableX) ? dockableX : !double.IsNaN(ownerX) ? ownerX : 0;
        }
        if (double.IsNaN(dockablePointerScreenY))
        {
            dockablePointerScreenY = !double.IsNaN(dockableY) ? dockableY : !double.IsNaN(ownerY) ? ownerY : 0;
        }
        if (double.IsNaN(dockableWidth))
        {
            dockableWidth = double.IsNaN(ownerWidth) ? 300 : ownerWidth;
        }
        if (double.IsNaN(dockableHeight))
        {
            dockableHeight = double.IsNaN(ownerHeight) ? 400 : ownerHeight;
        }

        SplitToWindow(dock, dockable, dockablePointerScreenX, dockablePointerScreenY, dockableWidth, dockableHeight);
    }

    /// <inheritdoc/>
    public virtual void CloseDockable(IDockable dockable)
    {
        if (dockable.OnClose())
        {
            RemoveDockable(dockable, true);
            OnDockableClosed(dockable);
        }
    }

    private void CloseDockablesRange(IDock dock, int start, int end, IDockable? excluding = null)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        for (var i = end; i >= start; --i)
        {
            if (excluding == null || dock.VisibleDockables[i] != excluding)
            {
                CloseDockable(dock.VisibleDockables[i]);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void CloseOtherDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        CloseDockablesRange(dock, 0, dock.VisibleDockables.Count - 1, dockable);
    }

    /// <inheritdoc/>
    public virtual void CloseAllDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        CloseDockablesRange(dock, 0, dock.VisibleDockables.Count - 1);
    }

    /// <inheritdoc/>
    public virtual void CloseLeftDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        int indexOf = dock.VisibleDockables.IndexOf(dockable);
        if (indexOf == -1)
        {
            return;
        }
            
        CloseDockablesRange(dock, 0, indexOf - 1);
    }

    /// <inheritdoc/>
    public virtual void CloseRightDockables(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        int indexOf = dock.VisibleDockables.IndexOf(dockable);
        if (indexOf == -1)
        {
            return;
        }
            
        CloseDockablesRange(dock, indexOf + 1, dock.VisibleDockables.Count - 1);
    }
}
