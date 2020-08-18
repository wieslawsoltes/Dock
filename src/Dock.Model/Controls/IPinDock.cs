
namespace Dock.Model.Controls
{
    /// <summary>
    /// Pin dock contract.
    /// </summary>
    public interface IPinDock : IDock
    {
        /// <summary>
        /// Gets or sets dock alignment.
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
    }
}
