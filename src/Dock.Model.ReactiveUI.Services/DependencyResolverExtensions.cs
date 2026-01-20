using System;
using Dock.Model.Services;
using Splat;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Provides Splat registration helpers for Dock overlay services.
/// </summary>
public static class DependencyResolverExtensions
{
    /// <summary>
    /// Registers Dock overlay services and default implementations.
    /// </summary>
    /// <param name="services">The Splat resolver.</param>
    /// <returns>The resolver.</returns>
    public static IMutableDependencyResolver RegisterDockOverlayServices(this IMutableDependencyResolver services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.RegisterLazySingletonIfMissing<IDockGlobalBusyService>(() => new DockGlobalBusyService());
        services.RegisterLazySingletonIfMissing<IDockGlobalDialogService>(() => new DockGlobalDialogService());
        services.RegisterLazySingletonIfMissing<IDockGlobalConfirmationService>(() => new DockGlobalConfirmationService());

        services.RegisterLazySingletonIfMissing<IHostServiceResolver>(() => new OwnerChainHostServiceResolver());
        services.RegisterLazySingletonIfMissing<Func<IHostOverlayServices>>(() => () => new HostOverlayServicesAdapter(
            new DockBusyService(services.GetRequiredService<IDockGlobalBusyService>()),
            new DockDialogService(services.GetRequiredService<IDockGlobalDialogService>()),
            new DockConfirmationService(services.GetRequiredService<IDockGlobalConfirmationService>()),
            services.GetRequiredService<IDockGlobalBusyService>(),
            services.GetRequiredService<IDockGlobalDialogService>(),
            services.GetRequiredService<IDockGlobalConfirmationService>()));
        services.RegisterLazySingletonIfMissing<IHostOverlayServicesProvider>(() => new HostOverlayServicesProvider(
            services.GetRequiredService<IHostServiceResolver>(),
            services.GetRequiredService<Func<IHostOverlayServices>>()));

        services.RegisterLazySingletonIfMissing<IWindowLifecycleService>(() => new DockWindowLifecycleService(
            services.GetRequiredService<IHostOverlayServicesProvider>(),
            services.GetRequiredService<IHostServiceResolver>()));
        services.RegisterLazySingletonIfMissing<IActivationBridge>(() => new DockActivationBridge());
        services.RegisterLazySingletonIfMissing<IDockDispatcher>(() => new MainThreadDispatcher());

        return services;
    }

    private static void RegisterLazySingletonIfMissing<TService>(
        this IMutableDependencyResolver services,
        Func<TService> factory)
        where TService : class
    {
        if (services.HasRegistration(typeof(TService), null))
        {
            return;
        }

        services.RegisterLazySingleton(factory);
    }

    private static TService GetRequiredService<TService>(this IMutableDependencyResolver resolver)
        where TService : class
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        if (resolver is IReadonlyDependencyResolver readOnlyResolver)
        {
            return readOnlyResolver.GetRequiredService<TService>();
        }

        var fallback = Locator.Current.GetService<TService>();
        if (fallback is null)
        {
            throw new InvalidOperationException($"Service '{typeof(TService).FullName}' is not registered.");
        }

        return fallback;
    }

    private static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
        where TService : class
    {
        var service = resolver.GetService<TService>();
        if (service is null)
        {
            throw new InvalidOperationException($"Service '{typeof(TService).FullName}' is not registered.");
        }

        return service;
    }
}
