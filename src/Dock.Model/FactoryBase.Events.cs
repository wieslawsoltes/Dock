// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Controls;
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
    public event EventHandler<GlobalDockTrackingChangedEventArgs>? GlobalDockTrackingChanged;

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
        var rootDock = ResolveRootDock(dockable);
        var window = rootDock?.Window;

        if (dockable is not null)
        {
            if (ShouldUpdateGlobalTrackingForRoot(rootDock))
            {
                UpdateGlobalDockTracking(dockable, rootDock, window, DockTrackingChangeReason.ActiveDockableChanged);
            }
        }
        else if (CurrentRootDock is { } currentRoot)
        {
            // Null active notifications are ambiguous without source context.
            // Keep tracking anchored to the current root instead of clearing it.
            var currentWindow = CurrentDockWindow ?? currentRoot.Window;
            var currentDockable = currentRoot.FocusedDockable ?? currentRoot.ActiveDockable;
            UpdateGlobalDockTracking(currentDockable, currentRoot, currentWindow, DockTrackingChangeReason.ActiveDockableChanged);
        }

        ActiveDockableChanged?.Invoke(this, new ActiveDockableChangedEventArgs(dockable, rootDock, window));
    }

    /// <inheritdoc />
    public virtual void OnFocusedDockableChanged(IDockable? dockable)
    {
        var rootDock = ResolveRootDock(dockable);
        var window = rootDock?.Window;

        if (dockable is not null)
        {
            if (ShouldUpdateGlobalTrackingForRoot(rootDock))
            {
                UpdateGlobalDockTracking(dockable, rootDock, window, DockTrackingChangeReason.FocusedDockableChanged);
            }
        }
        else if (CurrentRootDock is { } currentRoot)
        {
            // Null focus notifications are ambiguous without source context.
            // Keep tracking anchored to the current root instead of clearing it.
            var currentWindow = CurrentDockWindow ?? currentRoot.Window;
            var currentDockable = currentRoot.FocusedDockable ?? currentRoot.ActiveDockable;
            UpdateGlobalDockTracking(currentDockable, currentRoot, currentWindow, DockTrackingChangeReason.FocusedDockableChanged);
        }

        FocusedDockableChanged?.Invoke(this, new FocusedDockableChangedEventArgs(dockable, rootDock, window));
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
        ClearGlobalDockTrackingForWindow(window, DockTrackingChangeReason.WindowRemoved);
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
        ClearGlobalDockTrackingForWindow(window, DockTrackingChangeReason.WindowClosed);
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
        var rootDock = window?.Layout;
        var dockable = rootDock?.FocusedDockable ?? rootDock?.ActiveDockable;
        UpdateGlobalDockTracking(dockable, rootDock, window, DockTrackingChangeReason.WindowActivated);
        WindowActivated?.Invoke(this, new WindowActivatedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnDockableActivated(IDockable? dockable)
    {
        var rootDock = ResolveRootDock(dockable);
        var window = rootDock?.Window;

        if (dockable is not null)
        {
            if (ShouldUpdateGlobalTrackingForRoot(rootDock))
            {
                UpdateGlobalDockTracking(dockable, rootDock, window, DockTrackingChangeReason.DockableActivated);
            }
        }

        DockableActivated?.Invoke(this, new DockableActivatedEventArgs(dockable));
    }

    private bool ShouldUpdateGlobalTrackingForRoot(IRootDock? rootDock)
    {
        if (rootDock is null)
        {
            return false;
        }

        if (CurrentRootDock is null || ReferenceEquals(CurrentRootDock, rootDock))
        {
            return true;
        }

        return IsCurrentGlobalTrackingRootStale();
    }

    private bool IsCurrentGlobalTrackingRootStale()
    {
        if (CurrentRootDock is not { } currentRoot)
        {
            return false;
        }

        if (CurrentDockWindow is { } currentWindow)
        {
            if (!ReferenceEquals(currentWindow.Layout, currentRoot))
            {
                return true;
            }

            if (currentRoot.Window is not null && !ReferenceEquals(currentRoot.Window, currentWindow))
            {
                return true;
            }

            return false;
        }

        if (currentRoot.Window is not null)
        {
            return true;
        }

        if (DockControls.Count == 0)
        {
            return false;
        }

        foreach (var dockControl in DockControls)
        {
            if (ReferenceEquals(dockControl.Layout, currentRoot))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public virtual void OnWindowDeactivated(IDockWindow? window)
    {
        if (window is not null && ReferenceEquals(CurrentDockWindow, window))
        {
            UpdateGlobalDockTracking(null, null, null, DockTrackingChangeReason.WindowDeactivated);
        }

        WindowDeactivated?.Invoke(this, new WindowDeactivatedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnDockableDeactivated(IDockable? dockable)
    {
        if (dockable is not null && ReferenceEquals(CurrentDockable, dockable))
        {
            var rootDock = CurrentRootDock ?? ResolveRootDock(dockable);
            var nextDockable = rootDock?.FocusedDockable;
            if (ReferenceEquals(nextDockable, dockable))
            {
                nextDockable = rootDock?.ActiveDockable;
            }

            if (ReferenceEquals(nextDockable, dockable))
            {
                nextDockable = null;
            }

            UpdateGlobalDockTracking(nextDockable, rootDock, CurrentDockWindow, DockTrackingChangeReason.DockableDeactivated);
        }

        DockableDeactivated?.Invoke(this, new DockableDeactivatedEventArgs(dockable));
    }
}
