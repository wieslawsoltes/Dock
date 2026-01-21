using System;
using System.Runtime.CompilerServices;
using Dock.Model.Core;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Overlays.Hosting;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Lifecycle;

/// <summary>
/// Default window lifecycle service that cleans up overlay services.
/// </summary>
public sealed class DockWindowLifecycleService : IWindowLifecycleService
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IHostServiceResolver? _resolver;
    private readonly ConditionalWeakTable<IDockWindow, object> _cleanedWindows = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DockWindowLifecycleService"/> class.
    /// </summary>
    /// <param name="overlayServicesProvider">The host overlay services provider.</param>
    public DockWindowLifecycleService(IHostOverlayServicesProvider overlayServicesProvider)
        : this(overlayServicesProvider, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockWindowLifecycleService"/> class.
    /// </summary>
    /// <param name="overlayServicesProvider">The host overlay services provider.</param>
    /// <param name="resolver">The optional host service resolver.</param>
    public DockWindowLifecycleService(
        IHostOverlayServicesProvider overlayServicesProvider,
        IHostServiceResolver? resolver)
    {
        _overlayServicesProvider = overlayServicesProvider ?? throw new ArgumentNullException(nameof(overlayServicesProvider));
        _resolver = resolver;
    }

    /// <inheritdoc />
    public void OnWindowClosed(IDockWindow? window)
    {
        CleanupOverlays(window);
    }

    /// <inheritdoc />
    public void OnWindowRemoved(IDockWindow? window)
    {
        CleanupOverlays(window);
    }

    private void CleanupOverlays(IDockWindow? window)
    {
        if (window is null || window.Layout is not IScreen screen)
        {
            return;
        }

        if (!TryMarkCleaned(window))
        {
            return;
        }

        var overlays = ResolveOverlays(screen);
        if (overlays is null)
        {
            return;
        }

        overlays.Dialogs.CancelAll();
        overlays.Confirmations.CancelAll();

        if (window.Layout is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private bool TryMarkCleaned(IDockWindow window)
    {
        if (_cleanedWindows.TryGetValue(window, out _))
        {
            return false;
        }

        _cleanedWindows.Add(window, new object());
        return true;
    }

    private IHostOverlayServices? ResolveOverlays(IScreen screen)
    {
        if (screen is IHostOverlayServices hostOverlayServices)
        {
            return hostOverlayServices;
        }

        if (_resolver is not null)
        {
            var resolved = _resolver.Resolve<IHostOverlayServices>(screen);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        if (_overlayServicesProvider is HostOverlayServicesProvider hostProvider)
        {
            return hostProvider.TryGetCached(screen, out var cached)
                ? cached
                : null;
        }

        return _overlayServicesProvider.GetServices(screen);
    }
}
