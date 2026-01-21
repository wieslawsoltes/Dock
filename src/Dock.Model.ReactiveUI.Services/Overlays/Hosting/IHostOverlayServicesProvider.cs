using Dock.Model.Services;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Overlays.Hosting;

/// <summary>
/// Resolves overlay services for a host screen.
/// </summary>
public interface IHostOverlayServicesProvider
{
    /// <summary>
    /// Gets overlay services for the provided host screen.
    /// </summary>
    /// <param name="screen">The host screen.</param>
    /// <returns>The overlay services instance.</returns>
    IHostOverlayServices GetServices(IScreen screen);
}
