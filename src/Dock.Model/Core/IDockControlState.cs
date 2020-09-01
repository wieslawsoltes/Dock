
namespace Dock.Model
{
    /// <summary>
    /// Dock cotrol state contract.
    /// </summary>
    public interface IDockControlState
    {
        /// <summary>
        /// Gets or sets dock manager.
        /// </summary>
        IDockManager DockManager { get; set; }
    }
}
