using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Dockable removed event args.
    /// </summary>
    public class DockableRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets removed dockable.
        /// </summary>
        public IDockable? Dockable { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="DockableRemovedEventArgs"/> class.
        /// </summary>
        /// <param name="dockable">The removed dockable.</param>
        public DockableRemovedEventArgs(IDockable? dockable)
        {
            Dockable = dockable;
        }
    }
}
