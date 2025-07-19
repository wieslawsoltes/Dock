// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;
using Dock.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// <param name="configure">Factory options configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDock<TFactory, TSerializer>(this IServiceCollection services, Action<FactoryOptions>? configure = null)
        where TFactory : class, IFactory
        where TSerializer : class, IDockSerializer
    {
        services.AddSingleton<IDockState, DockState>();
        services.AddSingleton<TFactory>();
        services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<TFactory>());
        services.AddSingleton<TSerializer>();
        services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<TSerializer>());

        services.AddOptions<DockSettingsOptions>();
        services.AddSingleton<IPostConfigureOptions<DockSettingsOptions>, DockSettingsOptionsPostConfigurator>();

        var options = new FactoryOptions();
        configure?.Invoke(options);

        if (options.Layout is { })
        {
            services.AddSingleton(options.Layout);
        }

        return services;
    }

    /// <summary>
    /// Registers Dock services and adds <see cref="DockEventLogger"/>.
    /// </summary>
    /// <typeparam name="TFactory">Implementation of <see cref="IFactory"/>.</typeparam>
    /// <typeparam name="TSerializer">Implementation of <see cref="IDockSerializer"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Factory options configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDockWithLogger<TFactory, TSerializer>(this IServiceCollection services, Action<FactoryOptions>? configure = null)
        where TFactory : class, IFactory
        where TSerializer : class, IDockSerializer
    {
        services.AddDock<TFactory, TSerializer>(configure);
        services.AddSingleton<DockEventLogger>();
        return services;
    }
}
