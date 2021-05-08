using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Active dockable changed event args.
    /// </summary>
    public class ActiveDockableChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets active dockable.
        /// </summary>
        public IDockable? Dockable { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="ActiveDockableChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dockable">The active dockable.</param>
        public ActiveDockableChangedEventArgs(IDockable? dockable)
        {
            Dockable = dockable;
        }
    }
}
