
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
        Alignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets if the Dock is expanded.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets if the Dock auto hides dockable when pointer is not over.
        /// </summary>
        bool AutoHide { get; set; }

        /// <summary>
        /// Gets or sets if the tool Dock grip mode.
        /// </summary>
        GripMode GripMode { get; set; }
    }
}
