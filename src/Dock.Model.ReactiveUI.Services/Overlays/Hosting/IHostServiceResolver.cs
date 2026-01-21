using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Overlays.Hosting;

/// <summary>
/// Resolves services scoped to a host screen.
/// </summary>
public interface IHostServiceResolver
{
    /// <summary>
    /// Resolves a service from the host screen or its owner chain.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <param name="screen">The host screen.</param>
    /// <returns>The resolved service instance, or null if not found.</returns>
    TService? Resolve<TService>(IScreen screen) where TService : class;
}
