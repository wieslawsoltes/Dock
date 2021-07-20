using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window opened event args.
    /// </summary>
    public class WindowOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets opened window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowOpenedEventArgs"/> class.
        /// </summary>
        /// <param name="window">The opened window.</param>
        public WindowOpenedEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
