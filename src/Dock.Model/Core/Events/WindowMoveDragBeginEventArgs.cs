using System;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window begin dragging event args.
    /// </summary>
    public class WindowMoveDragBeginEventArgs : EventArgs
    {
        /// <summary>
        /// Gets dragged window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Gets or sets flag indicating whether window dragging should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowMoveDragBeginEventArgs"/> class.
        /// </summary>
        /// <param name="window">The dragged window.</param>
        public WindowMoveDragBeginEventArgs(IDockWindow? window)
        {
            Window = window;
        }
    }
}
