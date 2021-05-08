using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window added event args.
    /// </summary>
    public class WindowAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets added window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowAddedEventArgs"/> class.
        /// </summary>
        /// <param name="window">The added window.</param>
        public WindowAddedEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
