using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Dock.Model.Core
{
    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock : IDockable
    {
        /// <summary>
        /// Gets or sets visible dockables.
        /// </summary>
        [JsonInclude]
        IList<IDockable>? VisibleDockables { get; set; }

        /// <summary>
        /// Gets or sets hidden dockables.
        /// </summary>
        [JsonInclude]
        IList<IDockable>? HiddenDockables { get; set; }

        /// <summary>
        /// Gets or sets pinned dockables.
        /// </summary>
        [JsonInclude]
        IList<IDockable>? PinnedDockables { get; set; }

        /// <summary>
        /// Gets or sets active dockable.
        /// </summary>
        [JsonInclude]
        IDockable? ActiveDockable { get; set; }

        /// <summary>
        /// Gets or sets default dockable.
        /// </summary>
        [JsonInclude]
        IDockable? DefaultDockable { get; set; }

        /// <summary>
        /// Gets or sets the focused dockable.
        /// </summary>
        [JsonInclude]
        IDockable? FocusedDockable { get; set; }

        /// <summary> 
        /// Gets or sets splitter proportion. 
        /// </summary> 
        [JsonInclude]
        double Proportion { get; set; }

        /// <summary> 
        /// Gets or sets docking mode. 
        /// </summary> 
        [JsonInclude]
        DockMode Dock { get; set; }

        /// <summary>
        /// Gets or sets if the dockable is the currently active.
        /// </summary>
        [JsonInclude]
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets if the Dock collapses when all its children are removed.
        /// </summary>
        [JsonInclude]
        bool IsCollapsable { get; set; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        [JsonIgnore]
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        [JsonIgnore]
        bool CanGoForward { get; }

        /// <summary>
        /// Navigates to the most recent entry in back navigation history, if there is one.
        /// </summary>
        [JsonIgnore]
        ICommand GoBack { get; }

        /// <summary>
        /// Navigate to the most recent entry in forward navigation history, if there is one.
        /// </summary>
        [JsonIgnore]
        ICommand GoForward { get; }

        /// <summary>
        /// Navigate to content that is contained by an object.
        /// </summary>
        [JsonIgnore]
        ICommand Navigate { get; }

        /// <summary>
        /// Close layout.
        /// </summary>
        [JsonIgnore]
        ICommand Close { get; }
    }
}
