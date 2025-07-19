// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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

    /// <summary>
    /// Registers Dock model services and binds <see cref="FactoryOptions"/> from configuration.
    /// </summary>
    /// <typeparam name="TFactory">Implementation of <see cref="IFactory"/>.</typeparam>
    /// <typeparam name="TSerializer">Implementation of <see cref="IDockSerializer"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration section for options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDock<TFactory, TSerializer>(this IServiceCollection services, IConfiguration configuration)
        where TFactory : class, IFactory
        where TSerializer : class, IDockSerializer
    {
        services.Configure<FactoryOptions>(configuration);
        return services.AddDock<TFactory, TSerializer>();
    }
}
