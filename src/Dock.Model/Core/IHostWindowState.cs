
namespace Dock.Model.Core
{
    /// <summary>
    /// Host window state contract.
    /// </summary>
    public interface IHostWindowState
    {
        /// <summary>
        /// Gets or sets dock manager.
        /// </summary>
        IDockManager DockManager { get; set; }
    }
}
