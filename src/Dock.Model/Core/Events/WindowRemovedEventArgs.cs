using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window removed event args.
    /// </summary>
    public class WindowRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets removed window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowRemovedEventArgs"/> class.
        /// </summary>
        /// <param name="window">The removed window.</param>
        public WindowRemovedEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
