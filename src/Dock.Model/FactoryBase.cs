using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase : IFactory
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
    public virtual void AddWindow(IRootDock rootDock, IDockWindow window)
    {
        rootDock.Windows ??= CreateList<IDockWindow>();
        rootDock.Windows.Add(window);
        OnWindowAdded(window);
        InitDockWindow(window, rootDock);
    }

    /// <inheritdoc/>
    public virtual void RemoveWindow(IDockWindow window)
    {
        if (window.Owner is IRootDock rootDock)
        {
            window.Exit();
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);
        }
    }

    /// <inheritdoc/>
    public virtual void SetActiveDockable(IDockable dockable)
    {
        if (dockable.Owner is IDock dock)
        {
            dock.ActiveDockable = dockable;
        }
    }

    private void SetIsActive(IDockable dockable, bool active)
    {
        if (dockable is IDock dock)
        {
            dock.IsActive = active;
        }
    }

    /// <inheritdoc />
    public virtual void SetFocusedDockable(IDock dock, IDockable? dockable)
    {
        if (dock.ActiveDockable is not null && FindRoot(dock.ActiveDockable, x => x.IsFocusableRoot) is { } root)
        {
            if (root.FocusedDockable?.Owner is not null)
            {
                SetIsActive(root.FocusedDockable.Owner, false);
            }

            if (dockable is not null)
            {
                if (root.FocusedDockable != dockable)
                {
                    root.FocusedDockable = dockable;
                }
            }

            if (root.FocusedDockable?.Owner is not null)
            {
                SetIsActive(root.FocusedDockable.Owner, true);
            }
        }
    }

    private bool IsPinned(IDockable dockable, IRootDock rootDock)
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

    private bool IsDockPinned(IList<IDockable>? pinnedDockables, IDock dock)
    {
        if (pinnedDockables is not null && pinnedDockables.Count != 0)
        {
            foreach (var pinnedDockable in pinnedDockables)
            {
                if (pinnedDockable.Owner == dock)
                {
                    return true;
                }
            }
            return true;
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

                var isPinned = IsPinned(dockable, rootDock);

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
    public virtual void CollapseDock(IDock dock)
    {
        if (!dock.IsCollapsable || dock.VisibleDockables is null || dock.VisibleDockables.Count != 0)
        {
            return;
        }

        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is { })
        {
            if (dock is IToolDock toolDock)
            {
                if (toolDock.Alignment == Alignment.Left && IsDockPinned(rootDock.LeftPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Right && IsDockPinned(rootDock.RightPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Top && IsDockPinned(rootDock.TopPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Bottom && IsDockPinned(rootDock.BottomPinnedDockables, dock))
                {
                    return;
                }
            }
        }

        if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
        {
            var toRemove = new List<IDockable>();
            var dockIndex = ownerDock.VisibleDockables.IndexOf(dock);

            if (dockIndex >= 0)
            {
                var indexSplitterPrevious = dockIndex - 1;
                if (dockIndex > 0 && indexSplitterPrevious >= 0)
                {
                    var previousVisible = ownerDock.VisibleDockables[indexSplitterPrevious];
                    if (previousVisible is IProportionalDockSplitter splitterPrevious)
                    {
                        toRemove.Add(splitterPrevious);
                    }
                }

                var indexSplitterNext = dockIndex + 1;
                if (dockIndex < ownerDock.VisibleDockables.Count - 1 && indexSplitterNext >= 0)
                {
                    var nextVisible = ownerDock.VisibleDockables[indexSplitterNext];
                    if (nextVisible is IProportionalDockSplitter splitterNext)
                    {
                        toRemove.Add(splitterNext);
                    }
                }

                foreach (var removeVisible in toRemove)
                {
                    RemoveDockable(removeVisible, true);
                }
            }
        }

        if (dock is IRootDock rootDockDock && rootDockDock.Window is { })
        {
            RemoveWindow(rootDockDock.Window);
        }
        else
        {
            RemoveDockable(dock, true);
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

    /// <inheritdoc/>
    public virtual IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
    {
        IDock? split;

        if (dockable is IDock dockableDock)
        {
            split = dockableDock;
        }
        else
        {
            split = CreateProportionalDock();
            split.Id = nameof(IProportionalDock);
            split.Title = nameof(IProportionalDock);
            split.VisibleDockables = CreateList<IDockable>();
            if (split.VisibleDockables is not null)
            {
                split.VisibleDockables.Add(dockable);
                OnDockableAdded(dockable);
                split.ActiveDockable = dockable;
            }
        }

        var containerProportion = dock.Proportion;
        dock.Proportion = double.NaN;

        var layout = CreateProportionalDock();
        layout.Id = nameof(IProportionalDock);
        layout.Title = nameof(IProportionalDock);
        layout.VisibleDockables = CreateList<IDockable>();
        layout.Proportion = containerProportion;

        var splitter = CreateProportionalDockSplitter();
        splitter.Id = nameof(IProportionalDockSplitter);
        splitter.Title = nameof(IProportionalDockSplitter);

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
            {
                layout.Orientation = Orientation.Horizontal;
                break;
            }
            case DockOperation.Top:
            case DockOperation.Bottom:
            {
                layout.Orientation = Orientation.Vertical;
                break;
            }
        }

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Top:
            {
                if (layout.VisibleDockables is not null)
                {
                    layout.VisibleDockables.Add(split);
                    OnDockableAdded(split);
                    layout.ActiveDockable = split;
                }

                break;
            }
            case DockOperation.Right:
            case DockOperation.Bottom:
            {
                if (layout.VisibleDockables is not null)
                {
                    layout.VisibleDockables.Add(dock);
                    OnDockableAdded(dock);
                    layout.ActiveDockable = dock;
                }

                break;
            }
        }

        layout.VisibleDockables?.Add(splitter);
        OnDockableAdded(splitter);

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Top:
            {
                if (layout.VisibleDockables is not null)
                {
                    layout.VisibleDockables.Add(dock);
                    OnDockableAdded(dock);
                    layout.ActiveDockable = dock;
                }

                break;
            }
            case DockOperation.Right:
            case DockOperation.Bottom:
            {
                if (layout.VisibleDockables is not null)
                {
                    layout.VisibleDockables.Add(split);
                    OnDockableAdded(split);
                    layout.ActiveDockable = split;
                }

                break;
            }
        }

        return layout;
    }

    /// <inheritdoc/>
    public virtual void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
            case DockOperation.Top:
            case DockOperation.Bottom:
            {
                if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
                {
                    var index = ownerDock.VisibleDockables.IndexOf(dock);
                    if (index >= 0)
                    {
                        var layout = CreateSplitLayout(dock, dockable, operation);
                        ownerDock.VisibleDockables.RemoveAt(index);
                        OnDockableRemoved(dockable);
                        ownerDock.VisibleDockables.Insert(index, layout);
                        OnDockableAdded(dockable);
                        InitDockable(layout, ownerDock);
                        ownerDock.ActiveDockable = layout;
                    }
                }
                break;
            }
            default:
                throw new NotSupportedException($"Not supported split operation: {operation}.");
        }
    }

    /// <inheritdoc/>
    public virtual IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockable? target;
        bool topmost;

        switch (dockable)
        {
            case ITool:
            {
                target = CreateToolDock();
                target.Id = nameof(IToolDock);
                target.Title = nameof(IToolDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dock.VisibleDockables is not null)
                    {
                        dock.VisibleDockables.Add(dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                topmost = true;
                break;
            }
            case IDocument:
            {
                target = CreateDocumentDock();
                target.Id = nameof(IDocumentDock);
                target.Title = nameof(IDocumentDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dockable.Owner is IDocumentDock sourceDocumentDock)
                    {
                        if (target is IDocumentDock documentDock)
                        {
                            documentDock.CanCreateDocument = sourceDocumentDock.CanCreateDocument;

                            if (sourceDocumentDock is IDocumentDockContent sourceDocumentDockContent
                                && documentDock is IDocumentDockContent documentDockContent)
                            {
                                documentDockContent.DocumentTemplate = sourceDocumentDockContent.DocumentTemplate;
                            }
                        }
                    }
                    if (dock.VisibleDockables is not null)
                    {
                        dock.VisibleDockables.Add(dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                topmost = false;
                break;
            }
            case IToolDock:
            {
                target = dockable;
                topmost = true;
                break;
            }
            case IDocumentDock:
            {
                target = dockable;
                topmost = false;
                break;
            }
            case IProportionalDock proportionalDock:
            {
                target = proportionalDock;
                topmost = false;
                break;
            }
            case IDockDock dockDock:
            {
                target = dockDock;
                topmost = false;
                break;
            }
            case IRootDock rootDock:
            {
                target = rootDock.ActiveDockable;
                topmost = false;
                break;
            }
            default:
            {
                return null;
            }
        }

        var root = CreateRootDock();
        root.Id = nameof(IRootDock);
        root.Title = nameof(IRootDock);
        root.VisibleDockables = CreateList<IDockable>();
        if (root.VisibleDockables is not null && target is not null)
        {
            root.VisibleDockables.Add(target);
            OnDockableAdded(target);
            root.ActiveDockable = target;
            root.DefaultDockable = target;
        }
        root.Owner = null;

        var window = CreateDockWindow();
        window.Id = nameof(IDockWindow);
        window.Title = "";
        window.Width = double.NaN;
        window.Height = double.NaN;
        window.Topmost = topmost;
        window.Layout = root;

        root.Window = window;

        return window;
    }

    /// <inheritdoc/>
    public virtual void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height)
    {
        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is null)
        {
            return;
        }

        RemoveDockable(dockable, true);

        var window = CreateWindowFrom(dockable);
        if (window is not null)
        {
            AddWindow(rootDock, window);
            window.X = x;
            window.Y = y;
            window.Width = width;
            window.Height = height;
            window.Present(false);
        }
    }
}
