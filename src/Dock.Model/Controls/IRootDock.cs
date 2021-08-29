
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
        [JsonInclude]
        bool IsFocusableRoot { get; set; }

        /// <summary>
        /// Gets or sets owner window.
        /// </summary>
        [JsonInclude]
        IDockWindow? Window { get; set; }

        /// <summary>
        /// Gets or sets windows.
        /// </summary>
        [JsonInclude]
        IList<IDockWindow>? Windows { get; set; }

        /// <summary>
        /// Show windows.
        /// </summary>
        [JsonInclude]
        ICommand ShowWindows { get; }

        /// <summary>
        /// Exit windows.
        /// </summary>
        [JsonInclude]
        ICommand ExitWindows { get; }
    }
}
