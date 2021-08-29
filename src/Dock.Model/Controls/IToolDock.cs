
using System.Text.Json.Serialization;
using Dock.Model.Core;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool dock contract.
    /// </summary>
    public interface IToolDock : IDock
    {
        /// <summary>
        /// Gets or sets dock auto hide alignment.
        /// </summary>
        [JsonInclude]
        Alignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets if the Dock is expanded.
        /// </summary>
        [JsonInclude]
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets if the Dock auto hides dockable when pointer is not over.
        /// </summary>
        [JsonInclude]
        bool AutoHide { get; set; }

        /// <summary>
        /// Gets or sets if the tool Dock grip mode.
        /// </summary>
        [JsonInclude]
        GripMode GripMode { get; set; }
    }
}
