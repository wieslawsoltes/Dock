using Dock.Model.Core;

namespace Dock.Model.Services;

/// <summary>
/// Handles window lifecycle events for cleanup and service coordination.
/// </summary>
public interface IWindowLifecycleService
{
    /// <summary>
    /// Called when a window has been closed.
    /// </summary>
    /// <param name="window">The closed window.</param>
    void OnWindowClosed(IDockWindow? window);

    /// <summary>
    /// Called when a window has been removed from the layout.
    /// </summary>
    /// <param name="window">The removed window.</param>
    void OnWindowRemoved(IDockWindow? window);
}
