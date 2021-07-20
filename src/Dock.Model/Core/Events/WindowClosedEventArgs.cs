using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window closed event args.
    /// </summary>
    public class WindowClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets closed window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowClosedEventArgs"/> class.
        /// </summary>
        /// <param name="window">The closed window.</param>
        public WindowClosedEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
