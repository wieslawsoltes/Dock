using System;
using Dock.Model.Core.Events;

namespace Dock.Model.Core
{
    /// <summary>
    /// Dock factory contract.
    /// </summary>
    public partial interface IFactory
    {
        /// <summary>
        /// Active dockable changed event handler.
        /// </summary>
        event EventHandler<ActiveDockableChangedEventArgs>? ActiveDockableChanged;

        /// <summary>
        /// Focused dockable changed event handler.
        /// </summary>
        event EventHandler<FocusedDockableChangedEventArgs>? FocusedDockableChanged;

        /// <summary>
        /// Dockable added event handler.
        /// </summary>
        event EventHandler<DockableAddedEventArgs>? DockableAdded;

        /// <summary>
        /// Dockable removed event handler.
        /// </summary>
        event EventHandler<DockableRemovedEventArgs>? DockableRemoved;

        /// <summary>
        /// Dockable closed event handler.
        /// </summary>
        event EventHandler<DockableClosedEventArgs>? DockableClosed;

        /// <summary>
        /// Dockable moved event handler.
        /// </summary>
        event EventHandler<DockableMovedEventArgs>? DockableMoved;

        /// <summary>
        /// Dockable swapped event handler.
        /// </summary>
        event EventHandler<DockableSwappedEventArgs>? DockableSwapped;

        /// <summary>
        /// Dockable pinned event handler.
        /// </summary>
        event EventHandler<DockablePinnedEventArgs>? DockablePinned;

        /// <summary>
        /// Dockable unpinned event handler.
        /// </summary>
        event EventHandler<DockableUnpinnedEventArgs>? DockableUnpinned;

        /// <summary>
        /// Window opened event handler.
        /// </summary>
        event EventHandler<WindowOpenedEventArgs>? WindowOpened;

        /// <summary>
        /// Window closing event handler.
        /// </summary>
        event EventHandler<WindowClosingEventArgs>? WindowClosing;

        /// <summary>
        /// Window closed event handler.
        /// </summary>
        event EventHandler<WindowClosedEventArgs>? WindowClosed;

        /// <summary>
        /// Window added event handler.
        /// </summary>
        event EventHandler<WindowAddedEventArgs>? WindowAdded;

        /// <summary>
        /// Window removed event handler.
        /// </summary>
        event EventHandler<WindowRemovedEventArgs>? WindowRemoved;

        /// <summary>
        /// Window dragging begin event handler.
        /// </summary>
        event EventHandler<WindowMoveDragBeginEventArgs>? WindowMoveDragBegin;

        /// <summary>
        /// Window dragging event handler.
        /// </summary>
        event EventHandler<WindowMoveDragEventArgs>? WindowMoveDrag;

        /// <summary>
        /// Window dragging end event handler.
        /// </summary>
        event EventHandler<WindowMoveDragEndEventArgs>? WindowMoveDragEnd;

        /// <summary>
        /// Called when the active dockable changed.
        /// </summary>
        /// <param name="dockable">The activate dockable.</param>
        void OnActiveDockableChanged(IDockable? dockable);

        /// <summary>
        /// Called when the focused dockable changed.
        /// </summary>
        /// <param name="dockable">The focused dockable.</param>
        void OnFocusedDockableChanged(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been added.
        /// </summary>
        /// <param name="dockable">The added dockable.</param>
        void OnDockableAdded(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been removed.
        /// </summary>
        /// <param name="dockable">The removed dockable.</param>
        void OnDockableRemoved(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been closed.
        /// </summary>
        /// <param name="dockable">The closed dockable.</param>
        void OnDockableClosed(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been moved.
        /// </summary>
        /// <param name="dockable">The moved dockable.</param>
        void OnDockableMoved(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been swapped.
        /// </summary>
        /// <param name="dockable">The swapped dockable.</param>
        void OnDockableSwapped(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been pinned.
        /// </summary>
        /// <param name="dockable">The pinned dockable.</param>
        void OnDockablePinned(IDockable? dockable);

        /// <summary>
        /// Called when the dockable has been unpinned.
        /// </summary>
        /// <param name="dockable">The unpinned dockable.</param>
        void OnDockableUnpinned(IDockable? dockable);

        /// <summary>
        /// Called when the window has been opened.
        /// </summary>
        /// <param name="window">The opened window.</param>
        void OnWindowOpened(IDockWindow? window);

        /// <summary>
        /// Called when the window is closing.
        /// </summary>
        /// <param name="window">The closing window.</param>
        /// <returns>False if closing canceled, otherwise true.</returns>
        bool OnWindowClosing(IDockWindow? window);

        /// <summary>
        /// Called when the window has been closed.
        /// </summary>
        /// <param name="window">The closed window.</param>
        void OnWindowClosed(IDockWindow? window);

        /// <summary>
        /// Called when the window has been added.
        /// </summary>
        /// <param name="window">The added window.</param>
        void OnWindowAdded(IDockWindow? window);

        /// <summary>
        /// Called when the window has been removed.
        /// </summary>
        /// <param name="window">The removed window.</param>
        void OnWindowRemoved(IDockWindow? window);

        /// <summary>
        /// Called before the window dragging starts.
        /// </summary>
        /// <param name="window">The dragged window.</param>
        /// <returns>False if dragging canceled, otherwise true.</returns>
        bool OnWindowMoveDragBegin(IDockWindow? window);

        /// <summary>
        /// Called when the window is dragged.
        /// </summary>
        /// <param name="window">The dragged window.</param>
        void OnWindowMoveDrag(IDockWindow? window);

        /// <summary>
        /// Called after the window dragging ended.
        /// </summary>
        /// <param name="window">The dragged window.</param>
        void OnWindowMoveDragEnd(IDockWindow? window);
    }
}
