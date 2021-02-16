
using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Root dock contract.
    /// </summary>
    public interface IRootDock : IDock
    {
        /// <summary>
        /// Gets or sets if root dock is focusable.
        /// </summary>
        bool IsFocusableRoot { get; set; }

        /// <summary>
        /// Gets or sets owner window.
        /// </summary>
        IDockWindow? Window { get; set; }

        /// <summary>
        /// Gets or sets windows.
        /// </summary>
        IList<IDockWindow>? Windows { get; set; }

        /// <summary>
        /// Show windows.
        /// </summary>
        ICommand ShowWindows { get; }

        /// <summary>
        /// Exit windows.
        /// </summary>
        ICommand ExitWindows { get; }
    }
}
