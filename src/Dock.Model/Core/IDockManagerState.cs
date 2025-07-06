namespace Dock.Model.Core;

/// <summary>
/// Dock manager state contract.
/// </summary>
public interface IDockManagerState
{
    /// <summary>
    /// Gets or sets dock manager.
    /// </summary>
    IDockManager DockManager { get; set; }
}
