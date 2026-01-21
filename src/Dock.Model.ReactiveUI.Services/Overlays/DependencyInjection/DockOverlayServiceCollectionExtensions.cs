using System;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Lifecycle;
using Dock.Model.ReactiveUI.Services.Overlays.Hosting;
using Dock.Model.ReactiveUI.Services.Overlays.Services;
using Dock.Model.ReactiveUI.Services.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dock.Model.ReactiveUI.Services.Overlays.DependencyInjection;

/// <summary>
/// Provides registration helpers for Dock overlay services with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class DockOverlayServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dock overlay services and default implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDockOverlayServices(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<IDockGlobalBusyService, DockGlobalBusyService>();
        services.TryAddSingleton<IDockGlobalDialogService, DockGlobalDialogService>();
        services.TryAddSingleton<IDockGlobalConfirmationService, DockGlobalConfirmationService>();

        services.TryAddSingleton<IHostServiceResolver, OwnerChainHostServiceResolver>();
        services.TryAddSingleton<Func<IHostOverlayServices>>(sp => () => new HostOverlayServicesAdapter(
            new DockBusyService(sp.GetRequiredService<IDockGlobalBusyService>()),
            new DockDialogService(sp.GetRequiredService<IDockGlobalDialogService>()),
            new DockConfirmationService(sp.GetRequiredService<IDockGlobalConfirmationService>()),
            sp.GetRequiredService<IDockGlobalBusyService>(),
            sp.GetRequiredService<IDockGlobalDialogService>(),
            sp.GetRequiredService<IDockGlobalConfirmationService>()));
        services.TryAddSingleton<IHostOverlayServicesProvider>(sp => new HostOverlayServicesProvider(
            sp.GetRequiredService<IHostServiceResolver>(),
            sp.GetRequiredService<Func<IHostOverlayServices>>()));

        services.TryAddSingleton<IWindowLifecycleService>(sp => new DockWindowLifecycleService(
            sp.GetRequiredService<IHostOverlayServicesProvider>(),
            sp.GetRequiredService<IHostServiceResolver>()));
        services.TryAddSingleton<IActivationBridge, DockActivationBridge>();
        services.TryAddSingleton<IDockDispatcher, MainThreadDispatcher>();

        return services;
    }
}
