namespace Dock.Avalonia.Controls;

/// <summary>
/// Defines a contract for hosting <see cref="OverlayHostWindow"/> instances.
/// </summary>
public interface IOverlayHost
{
    /// <summary>
    /// Adds the specified overlay window to the host.
    /// </summary>
    /// <param name="window">The overlay window.</param>
    void Add(OverlayHostWindow window);

    /// <summary>
    /// Removes the specified overlay window from the host.
    /// </summary>
    /// <param name="window">The overlay window.</param>
    void Remove(OverlayHostWindow window);
}

/// <summary>
/// Provides global access to an overlay host.
/// </summary>
public static class OverlayHost
{
    /// <summary>
    /// Gets or sets the current overlay host.
    /// </summary>
    public static IOverlayHost? Current { get; set; }
}
