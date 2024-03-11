using System.Diagnostics;
using System.Linq;
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
        AddVisibleDockable(dock, dockable);
        OnDockableAdded(dockable);
    }

    /// <inheritdoc/>
    public virtual void InsertDockable(IDock dock, IDockable dockable, int index)
    {
        if (index >= 0)
        {
            InitDockable(dockable, dock);
            dock.VisibleDockables ??= CreateList<IDockable>();
            InsertVisibleDockable(dock, index, dockable);
            OnDockableAdded(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveDockable(IDockable dockable, bool collapse)
    {
        // to correctly remove a pinned dockable, it needs to be unpinned
        UnpinDockable(dockable);

        if (dockable.Owner is not IDock dock || dock.VisibleDockables is null)
        {
            return;
        }

        var index = dock.VisibleDockables.IndexOf(dockable);
        if (index < 0)
        {
            return;
        }

        RemoveVisibleDockable(dock, dockable);
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
            RemoveVisibleDockableAt(dock, sourceIndex);
            OnDockableRemoved(sourceDockable);
            InsertVisibleDockable(dock, targetIndex, sourceDockable);
            OnDockableAdded(sourceDockable);
            OnDockableMoved(sourceDockable);
            dock.ActiveDockable = sourceDockable;
        }
    }

    /// <inheritdoc/>
    public virtual void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        UnpinDockable(sourceDockable);

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
                    InsertVisibleDockable(targetDock, targetIndex + 1, sourceDockable);
                    OnDockableAdded(sourceDockable);
                    RemoveVisibleDockableAt(targetDock, sourceIndex);
                    OnDockableRemoved(sourceDockable);
                    OnDockableMoved(sourceDockable);
                }
                else
                {
                    var removeIndex = sourceIndex + 1;
                    if (targetDock.VisibleDockables.Count + 1 > removeIndex)
                    {
                        InsertVisibleDockable(targetDock, targetIndex, sourceDockable);
                        OnDockableAdded(sourceDockable);
                        RemoveVisibleDockableAt(targetDock, removeIndex);
                        OnDockableRemoved(sourceDockable);
                        OnDockableMoved(sourceDockable);
                    }
                }
            }
            else
            {
                RemoveDockable(sourceDockable, true);
                InsertVisibleDockable(targetDock, targetIndex, sourceDockable);
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

    /// <inheritdoc/>
    public bool IsDockablePinned(IDockable dockable, IRootDock? rootDock = null)
    {
        if (rootDock == null)
        {
            rootDock = FindRoot(dockable);

            if (rootDock == null)
            {
                return false;
            }
        }

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
    public void HidePreviewingDockables(IRootDock rootDock)
    {
        if (rootDock.PinnedDock == null)
            return;

        if (rootDock.PinnedDock.VisibleDockables != null)
        {
            foreach (var dockable in rootDock.PinnedDock.VisibleDockables)
            {
                dockable.Owner = dockable.OriginalOwner;
                dockable.OriginalOwner = null;
            }
            RemoveAllVisibleDockables(rootDock.PinnedDock);
        }
    }

    /// <inheritdoc/>
    public void PreviewPinnedDockable(IDockable dockable)
    {
        var rootDock = FindRoot(dockable, _ => true);
        if (rootDock is null)
        {
            return;
        }

        HidePreviewingDockables(rootDock);

        var alignment = (dockable.Owner as IToolDock)?.Alignment ?? Alignment.Unset;

        if (rootDock.PinnedDock == null)
        {
            rootDock.PinnedDock = CreateToolDock();
            InitDockable(rootDock.PinnedDock, rootDock);
        }
        rootDock.PinnedDock.Alignment = alignment;

        Debug.Assert(rootDock.PinnedDock != null);

        RemoveAllVisibleDockables(rootDock.PinnedDock);

        dockable.OriginalOwner = dockable.Owner;
        AddVisibleDockable(rootDock.PinnedDock, dockable);
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

                var originalToolDock = dockable.OriginalOwner as IToolDock;

                var alignment = originalToolDock?.Alignment ?? toolDock.Alignment;

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
                        RemoveVisibleDockable(toolDock, dockable);
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
                else if (isPinned)
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

                    if (!isVisible)
                    {
                        AddVisibleDockable(toolDock, dockable);
                    }
                    else
                    {
                        Debug.Assert(dockable.OriginalOwner is IDock);
                        var originalOwner = (IDock)dockable.OriginalOwner;
                        HidePreviewingDockables(rootDock);
                        AddVisibleDockable(originalOwner, dockable);
                    }

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
    public void UnpinDockable(IDockable dockable)
    {
        if (IsDockablePinned(dockable))
        {
            PinDockable(dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void FloatDockable(IDockable dockable)
    {
        if (dockable.Owner is not IDock dock)
        {
            return;
        }

        UnpinDockable(dockable);

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
        if (dockable.CanClose && dockable.OnClose())
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

    /// <summary>
    /// Adds the dockable to the visible dockables list of the dock.
    /// </summary>
    protected void AddVisibleDockable(IDock dock, IDockable dockable)
    {
        if (dock.VisibleDockables == null)
        {
            dock.VisibleDockables = CreateList<IDockable>();
        }
        dock.VisibleDockables.Add(dockable);
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Inserts the dockable to the visible dockables list of the dock at the specified index.
    /// </summary>
    protected void InsertVisibleDockable(IDock dock, int index, IDockable dockable)
    {
        if (dock.VisibleDockables == null)
        {
            dock.VisibleDockables = CreateList<IDockable>();
        }
        dock.VisibleDockables.Insert(index, dockable);
        UpdateIsEmpty(dock);
    }

    /// <summary>
    /// Removes the dockable from the visible dockables list of the dock.
    /// </summary>
    protected void RemoveVisibleDockable(IDock dock, IDockable dockable)
    {
        if (dock.VisibleDockables != null)
        {
            dock.VisibleDockables.Remove(dockable);
            UpdateIsEmpty(dock);
        }
    }

    /// <summary>
    /// Removes all visible dockable of the dock.
    /// </summary>
    protected void RemoveAllVisibleDockables(IDock dock)
    {
        if (dock.VisibleDockables != null)
        {
            if (dock.VisibleDockables.Count > 0)
            {
                dock.VisibleDockables.Clear();
                UpdateIsEmpty(dock);
            }
        }
    }

    /// <summary>
    /// Removes the dockable at the specified index from the visible dockables list of the dock.
    /// </summary>
    protected void RemoveVisibleDockableAt(IDock dock, int index)
    {
        if (dock.VisibleDockables != null)
        {
            dock.VisibleDockables.RemoveAt(index);
            UpdateIsEmpty(dock);
        }
    }

    private void UpdateIsEmpty(IDock dock)
    {
        bool oldIsEmpty = dock.IsEmpty;

        var newIsEmpty = dock.VisibleDockables == null
                         || dock.VisibleDockables?.Count == 0
                         || dock.VisibleDockables!.All(x => x is IDock { IsEmpty: true } or IProportionalDockSplitter);

        if (oldIsEmpty != newIsEmpty)
        {
            dock.IsEmpty = newIsEmpty;
            if (dock.Owner is IDock parent)
                UpdateIsEmpty(parent);
        }

        UpdateOpenedDockablesCount(dock);
    }

    private void UpdateOpenedDockablesCount(IDockable dockable)
    {
        switch (dockable)
        {
            case IProportionalDock proportionalDock:
                proportionalDock.OpenedDockablesCount = proportionalDock.VisibleDockables?.Sum(x => (x as IDock)?.OpenedDockablesCount ?? 0) ?? 0;
                break;
            case IRootDock rootDock:
                rootDock.OpenedDockablesCount = rootDock.VisibleDockables?.Sum(x => (x as IDock)?.OpenedDockablesCount ?? 0) ?? 0;
                break;
            case IDock dock:
                dock.OpenedDockablesCount = 1;
                break;
            default:
                break;
        }

        if (dockable.Owner != null)
            UpdateOpenedDockablesCount(dockable.Owner);
    }
}
