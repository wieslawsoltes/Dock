using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Dockable pinned event args.
    /// </summary>
    public class DockablePinnedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets pinned dockable.
        /// </summary>
        public IDockable? Dockable { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="DockablePinnedEventArgs"/> class.
        /// </summary>
        /// <param name="dockable">The pinned dockable.</param>
        public DockablePinnedEventArgs(IDockable? dockable)
        {
            Dockable = dockable;
        }
    }
}
