using System;
using System.Runtime.CompilerServices;
using Dock.Model.Core;
using Dock.Model.Services;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Overlays.Hosting;

/// <summary>
/// Default overlay services resolver that uses the host service resolver.
/// </summary>
public sealed class HostOverlayServicesProvider : IHostOverlayServicesProvider
{
    private readonly IHostServiceResolver _resolver;
    private readonly Func<IHostOverlayServices> _fallbackFactory;
    private readonly ConditionalWeakTable<IScreen, IHostOverlayServices> _fallbackCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HostOverlayServicesProvider"/> class.
    /// </summary>
    /// <param name="resolver">The host service resolver.</param>
    /// <param name="fallback">The fallback overlay services.</param>
    public HostOverlayServicesProvider(IHostServiceResolver resolver, IHostOverlayServices fallback)
        : this(resolver, () => fallback)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HostOverlayServicesProvider"/> class.
    /// </summary>
    /// <param name="resolver">The host service resolver.</param>
    /// <param name="fallbackFactory">The fallback overlay services factory.</param>
    public HostOverlayServicesProvider(IHostServiceResolver resolver, Func<IHostOverlayServices> fallbackFactory)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _fallbackFactory = fallbackFactory ?? throw new ArgumentNullException(nameof(fallbackFactory));
    }

    /// <inheritdoc />
    public IHostOverlayServices GetServices(IScreen screen)
    {
        if (screen is null)
        {
            throw new ArgumentNullException(nameof(screen));
        }

        var resolved = _resolver.Resolve<IHostOverlayServices>(screen);
        if (resolved is not null)
        {
            _fallbackCache.Remove(screen);
            return resolved;
        }

        return _fallbackCache.GetValue(screen, _ => _fallbackFactory());
    }

    /// <inheritdoc />
    public IHostOverlayServices GetServices(IScreen screen, IDockable dockable)
    {
        if (screen is null)
        {
            throw new ArgumentNullException(nameof(screen));
        }

        if (dockable is null)
        {
            throw new ArgumentNullException(nameof(dockable));
        }

        var resolved = ResolveFromDockable(dockable);
        return resolved ?? GetServices(screen);
    }

    internal bool TryGetCached(IScreen screen, out IHostOverlayServices services)
    {
        if (screen is null)
        {
            throw new ArgumentNullException(nameof(screen));
        }

        if (_fallbackCache.TryGetValue(screen, out var cached))
        {
            services = cached;
            return true;
        }

        services = null!;
        return false;
    }

    private IHostOverlayServices? ResolveFromDockable(IDockable dockable)
    {
        if (dockable is IScreen dockableScreen)
        {
            var resolved = _resolver.Resolve<IHostOverlayServices>(dockableScreen);
            if (resolved is not null)
            {
                _fallbackCache.Remove(dockableScreen);
                return resolved;
            }
        }

        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return null;
        }

        var root = factory.FindRoot(dockable, _ => true);
        if (root is IHostOverlayServices hostServices)
        {
            return hostServices;
        }

        if (root?.Window?.Layout is IHostOverlayServices layoutServices)
        {
            return layoutServices;
        }

        return null;
    }

    private static IFactory? FindFactory(IDockable dockable)
    {
        var current = dockable;
        while (current is not null)
        {
            if (current.Factory is IFactory factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }
}
