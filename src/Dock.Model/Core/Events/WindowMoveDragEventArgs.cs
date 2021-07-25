using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window dragging event args.
    /// </summary>
    public class WindowMoveDragEventArgs : EventArgs
    {
        /// <summary>
        /// Gets dragged window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowMoveDragEventArgs"/> class.
        /// </summary>
        /// <param name="window">The dragged window.</param>
        public WindowMoveDragEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
