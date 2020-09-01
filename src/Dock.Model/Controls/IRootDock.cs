
using System.Collections.Generic;

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
        void ShowWindows();

        /// <summary>
        /// Exit windows.
        /// </summary>
        void ExitWindows();
    }
}
