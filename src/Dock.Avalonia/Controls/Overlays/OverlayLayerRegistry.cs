using System;
using System.Collections.Generic;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Provides access to overlay layer registrations from DI containers.
/// </summary>
public static class OverlayLayerRegistry
{
    private static Func<IEnumerable<IOverlayLayer>>? _provider;

    /// <summary>
    /// Raised when the layer provider changes.
    /// </summary>
    public static event EventHandler? ProviderChanged;

    /// <summary>
    /// Gets or sets the provider used to resolve overlay layers.
    /// </summary>
    public static Func<IEnumerable<IOverlayLayer>>? Provider
    {
        get => _provider;
        set
        {
            if (_provider == value)
            {
                return;
            }

            _provider = value;
            ProviderChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Configures the registry to resolve layers from the given service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve layers.</param>
    public static void UseServiceProvider(IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
        {
            Provider = null;
            return;
        }

        Provider = () => serviceProvider.GetService(typeof(IEnumerable<IOverlayLayer>)) as IEnumerable<IOverlayLayer>
            ?? Array.Empty<IOverlayLayer>();
    }
}
