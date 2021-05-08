using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Dockable unpinned event args.
    /// </summary>
    public class DockableUnpinnedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets unpinned dockable.
        /// </summary>
        public IDockable? Dockable { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="DockableUnpinnedEventArgs"/> class.
        /// </summary>
        /// <param name="dockable">The unpinned dockable.</param>
        public DockableUnpinnedEventArgs(IDockable? dockable)
        {
            Dockable = dockable;
        }
    }
}
