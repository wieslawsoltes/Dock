using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Focused dockable changed event args.
    /// </summary>
    public class FocusedDockableChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets focused dockable.
        /// </summary>
        public IDockable? Dockable { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="FocusedDockableChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dockable">The focused dockable.</param>
        public FocusedDockableChangedEventArgs(IDockable? dockable)
        {
            Dockable = dockable;
        }
    }
}
