/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
    public event EventHandler<DockableClosedEventArgs>? DockableClosed;

    /// <inheritdoc />
    public event EventHandler<DockableMovedEventArgs>? DockableMoved;

    /// <inheritdoc />
    public event EventHandler<DockableSwappedEventArgs>? DockableSwapped;

    /// <inheritdoc />
    public event EventHandler<DockablePinnedEventArgs>? DockablePinned;

    /// <inheritdoc />
    public event EventHandler<DockableUnpinnedEventArgs>? DockableUnpinned;

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
    public virtual void OnWindowAdded(IDockWindow? window)
    {
        WindowAdded?.Invoke(this, new WindowAddedEventArgs(window));
    }

    /// <inheritdoc />
    public virtual void OnWindowRemoved(IDockWindow? window)
    {
        WindowRemoved?.Invoke(this, new WindowRemovedEventArgs(window));
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
}
