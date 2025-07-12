using Dock.Model;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dock.Model.Extensions.DependencyInjection;

/// <summary>
/// Provides registration helpers for Dock model services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dock model services using the provided factory and serializer types.
    /// </summary>
    /// <typeparam name="TFactory">Implementation of <see cref="IFactory"/>.</typeparam>
    /// <typeparam name="TSerializer">Implementation of <see cref="IDockSerializer"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDock<TFactory, TSerializer>(this IServiceCollection services)
        where TFactory : class, IFactory
        where TSerializer : class, IDockSerializer
    {
        services.AddSingleton<IDockState, DockState>();
        services.AddSingleton<TFactory>();
        services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<TFactory>());
        services.AddSingleton<TSerializer>();
        services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<TSerializer>());
        return services;
    }
}
