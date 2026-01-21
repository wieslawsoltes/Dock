namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Creates overlay layers on demand to avoid sharing instances between hosts.
/// </summary>
public interface IOverlayLayerFactory
{
    /// <summary>
    /// Creates a new overlay layer instance.
    /// </summary>
    /// <returns>The created overlay layer.</returns>
    IOverlayLayer Create();
}
