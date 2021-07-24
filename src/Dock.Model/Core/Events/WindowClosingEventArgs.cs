using System;
using System.Collections.Generic;
using System.Text;

namespace Dock.Model.Core.Events
{
    /// <summary>
    /// Window closing event args.
    /// </summary>
    public class WindowClosingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets removed window.
        /// </summary>
        public IDockWindow? Window { get; }

        /// <summary>
        /// Gets or sets cancel
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="WindowClosingEventArgs"/> class.
        /// </summary>
        /// <param name="window">The removed window.</param>
        public WindowClosingEventArgs(IDockWindow? window)
        {
            Window = window;
            Cancel = false;
        }
    }
}
