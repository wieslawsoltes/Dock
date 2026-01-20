// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Core;
using Dock.Model.Core.Events;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc />
    public event EventHandler<ActiveDockableChangedEventArgs>? ActiveDockableChanged;

    /// <inheritdoc />
    public event EventHandler<FocusedDockableChangedEventArgs>? FocusedDockableChanged;

    /// <inheritdoc />
    public event EventHandler<DockableInitEventArgs>? DockableInit;

    /// <inheritdoc />
    public event EventHandler<DockableAddedEventArgs>? DockableAdded;

    /// <inheritdoc />
    public event EventHandler<DockableRemovedEventArgs>? DockableRemoved;

    /// <inheritdoc />
    public event EventHandler<DockableClosingEventArgs>? DockableClosing;

    /// <inheritdoc />
    public event EventHandler<DockableClosedEventArgs>? DockableClosed;

    /// <inheritdoc />
    public event EventHandler<DockableMovedEventArgs>? DockableMoved;

    /// <inheritdoc />
    public event EventHandler<DockableDockedEventArgs>? DockableDocked;

    /// <inheritdoc />
    public event EventHandler<DockableUndockedEventArgs>? DockableUndocked;

    /// <inheritdoc />
    public event EventHandler<DockableSwappedEventArgs>? DockableSwapped;

    /// <inheritdoc />
    public event EventHandler<DockablePinnedEventArgs>? DockablePinned;

    /// <inheritdoc />
    public event EventHandler<DockableUnpinnedEventArgs>? DockableUnpinned;

    /// <inheritdoc />
    public event EventHandler<DockableHiddenEventArgs>? DockableHidden;

    /// <inheritdoc />
    public event EventHandler<DockableRestoredEventArgs>? DockableRestored;

    /// <inheritdoc />
    public event EventHandler<WindowOpenedEventArgs>? WindowOpened;

    /// <inheritdoc />
    public event EventHandler<WindowClosingEventArgs>? WindowClosing;

    /// <inheritdoc />
    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    /// <inheritdoc />
    public event EventHandler<WindowAddedEventArgs>? WindowAdded;

    /// <inheritdoc />
    public event EventHandler<WindowRemovedEventArgs>? WindowRemoved;

    /// <inheritdoc />
    public event EventHandler<WindowMoveDragBeginEventArgs>? WindowMoveDragBegin;

    /// <inheritdoc />
    public event EventHandler<WindowMoveDragEventArgs>? WindowMoveDrag;

    /// <inheritdoc />
    public event EventHandler<WindowMoveDragEndEventArgs>? WindowMoveDragEnd;

    /// <inheritdoc />
    public event EventHandler<WindowActivatedEventArgs>? WindowActivated;

    /// <inheritdoc />
    public event EventHandler<DockableActivatedEventArgs>? DockableActivated;

    /// <inheritdoc />
    public event EventHandler<WindowDeactivatedEventArgs>? WindowDeactivated;

    /// <inheritdoc />
    public event EventHandler<DockableDeactivatedEventArgs>? DockableDeactivated;

    /// <inheritdoc />
    public virtual void OnActiveDockableChanged(IDockable? dockable)
    {
        ActiveDockableChanged?.Invoke(this, new ActiveDockableChangedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnFocusedDockableChanged(IDockable? dockable)
    {
        FocusedDockableChanged?.Invoke(this, new FocusedDockableChangedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableAdded(IDockable? dockable)
    {
        DockableAdded?.Invoke(this, new DockableAddedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableRemoved(IDockable? dockable)
    {
        DockableRemoved?.Invoke(this, new DockableRemovedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual bool OnDockableClosing(IDockable? dockable)
    {
        var canClose = dockable?.OnClose() ?? true;

        var eventArgs = new DockableClosingEventArgs(dockable)
        {
            Cancel = !canClose
        };

        DockableClosing?.Invoke(this, eventArgs);

        return !eventArgs.Cancel;
    }

    /// <inheritdoc />
    public virtual void OnDockableClosed(IDockable? dockable)
    {
        DockableClosed?.Invoke(this, new DockableClosedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableMoved(IDockable? dockable)
    {
        DockableMoved?.Invoke(this, new DockableMovedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableDocked(IDockable? dockable, DockOperation operation)
    {
        DockableDocked?.Invoke(this, new DockableDockedEventArgs(dockable, operation));
    }

    /// <inheritdoc />
    public virtual void OnDockableUndocked(IDockable? dockable, DockOperation operation)
    {
        DockableUndocked?.Invoke(this, new DockableUndockedEventArgs(dockable, operation));
    }

    /// <inheritdoc />
    public virtual void OnDockableSwapped(IDockable? dockable)
    {
        DockableSwapped?.Invoke(this, new DockableSwappedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockablePinned(IDockable? dockable)
    {
        DockablePinned?.Invoke(this, new DockablePinnedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableUnpinned(IDockable? dockable)
    {
        DockableUnpinned?.Invoke(this, new DockableUnpinnedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableHidden(IDockable? dockable)
    {
        DockableHidden?.Invoke(this, new DockableHiddenEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnDockableRestored(IDockable? dockable)
    {
        DockableRestored?.Invoke(this, new DockableRestoredEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnWindowAdded(IDockWindow? window)
    {
        WindowAdded?.Invoke(this, new WindowAddedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnWindowRemoved(IDockWindow? window)
    {
        WindowRemoved?.Invoke(this, new WindowRemovedEventArgs(window));
        NotifyWindowRemoved(window);
    }

    /// <inheritdoc />
    public virtual void OnWindowOpened(IDockWindow? window)
    {
        WindowOpened?.Invoke(this, new WindowOpenedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnDockableInit(IDockable? dockable)
    {
        var eventArgs = new DockableInitEventArgs(dockable)
        {
            Context = dockable?.Context
        };

        DockableInit?.Invoke(this, eventArgs);

        if (dockable is { } && eventArgs.Context != dockable.Context)
        {
            dockable.Context = eventArgs.Context;
        }
    }

    /// <inheritdoc />
    public virtual bool OnWindowClosing(IDockWindow? window)
    {
        var canClose = window?.OnClose() ?? true;

        var eventArgs = new WindowClosingEventArgs(window)
        {
            Cancel = !canClose
        };

        WindowClosing?.Invoke(this, eventArgs);

        return !eventArgs.Cancel;
    }

    /// <inheritdoc />
    public virtual void OnWindowClosed(IDockWindow? window)
    {
        WindowClosed?.Invoke(this, new WindowClosedEventArgs(window));
        NotifyWindowClosed(window);
    }

    /// <inheritdoc />
    public virtual bool OnWindowMoveDragBegin(IDockWindow? window)
    {
        var canMoveDrag = window?.OnMoveDragBegin() ?? true;

        var eventArgs = new WindowMoveDragBeginEventArgs(window)
        {
            Cancel = !canMoveDrag
        };

        WindowMoveDragBegin?.Invoke(this, eventArgs);

        return !eventArgs.Cancel;
    }

    /// <inheritdoc />
    public virtual void OnWindowMoveDrag(IDockWindow? window)
    {
        window?.OnMoveDrag();
        WindowMoveDrag?.Invoke(this, new WindowMoveDragEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnWindowMoveDragEnd(IDockWindow? window)
    {
        window?.OnMoveDragEnd();
        WindowMoveDragEnd?.Invoke(this, new WindowMoveDragEndEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnWindowActivated(IDockWindow? window)
    {
        WindowActivated?.Invoke(this, new WindowActivatedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnDockableActivated(IDockable? dockable)
    {
        DockableActivated?.Invoke(this, new DockableActivatedEventArgs(dockable));
    }

    /// <inheritdoc />
    public virtual void OnWindowDeactivated(IDockWindow? window)
    {
        WindowDeactivated?.Invoke(this, new WindowDeactivatedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnDockableDeactivated(IDockable? dockable)
    {
        DockableDeactivated?.Invoke(this, new DockableDeactivatedEventArgs(dockable));
    }
}
