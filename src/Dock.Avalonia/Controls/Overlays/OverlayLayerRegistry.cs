using System;
using System.Collections.Generic;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Provides access to overlay layer registrations from DI containers.
/// </summary>
public static class OverlayLayerRegistry
{
    private static Func<IEnumerable<IOverlayLayer>>? _provider;
    private static Func<IEnumerable<IOverlayLayerFactory>>? _factoryProvider;

    /// <summary>
    /// Raised when the layer provider changes.
    /// </summary>
    public static event EventHandler? ProviderChanged;

    /// <summary>
    /// Gets or sets the provider used to resolve overlay layer factories.
    /// </summary>
    public static Func<IEnumerable<IOverlayLayerFactory>>? FactoryProvider
    {
        get => _factoryProvider;
        set => UpdateProviders(_provider, value);
    }

    /// <summary>
    /// Gets or sets the provider used to resolve overlay layers.
    /// </summary>
    public static Func<IEnumerable<IOverlayLayer>>? Provider
    {
        get => _provider;
        set => UpdateProviders(value, _factoryProvider);
    }

    /// <summary>
    /// Configures the registry to resolve layers from the given service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve layers.</param>
    public static void UseServiceProvider(IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
        {
            UpdateProviders(null, null);
            return;
        }

        UpdateProviders(
            () => serviceProvider.GetService(typeof(IEnumerable<IOverlayLayer>)) as IEnumerable<IOverlayLayer>
                ?? Array.Empty<IOverlayLayer>(),
            () => serviceProvider.GetService(typeof(IEnumerable<IOverlayLayerFactory>)) as IEnumerable<IOverlayLayerFactory>
                ?? Array.Empty<IOverlayLayerFactory>());
    }

    private static void UpdateProviders(
        Func<IEnumerable<IOverlayLayer>>? provider,
        Func<IEnumerable<IOverlayLayerFactory>>? factoryProvider)
    {
        if (_provider == provider && _factoryProvider == factoryProvider)
        {
            return;
        }

        _provider = provider;
        _factoryProvider = factoryProvider;
        ProviderChanged?.Invoke(null, EventArgs.Empty);
    }
}
