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
        public event EventHandler<ActiveDockableChangedEventArgs> ActiveDockableChanged;

        /// <summary>
        /// Focused dockable changed event handler.
        /// </summary>
        public event EventHandler<FocusedDockableChangedEventArgs> FocusedDockableChanged;

        /// <summary>
        /// Called when the active dockable changed.
        /// </summary>
        /// <param name="dockable">The activate dockable.</param>
        void OnActiveDockableChanged(IDockable dockable);

        /// <summary>
        /// Called when the focused dockable changed.
        /// </summary>
        /// <param name="dockable">The focused dockable.</param>
        void OnFocusedDockableChanged(IDockable dockable);
    }
}
