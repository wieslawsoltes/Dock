using System;
using System.Linq;
using System.Threading.Tasks;
using Dock.Model.Core;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Overlays.Controls;

/// <summary>
/// Root dock that binds overlay services and supports reload handling.
/// </summary>
public sealed class BusyRootDock : RoutableRootDock, IHostOverlayServices, IDisposable
{
    private readonly IHostOverlayServices _hostServices;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusyRootDock"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="hostServicesFactory">Factory used to create overlay services.</param>
    /// <param name="url">Optional router URL segment.</param>
    public BusyRootDock(
        IScreen host,
        Func<IHostOverlayServices> hostServicesFactory,
        string? url = null)
        : base(host, url)
    {
        if (hostServicesFactory is null)
        {
            throw new ArgumentNullException(nameof(hostServicesFactory));
        }

        _hostServices = hostServicesFactory();
        Busy.SetReloadHandler(ReloadCurrentAsync);
    }

    /// <inheritdoc />
    public IDockBusyService Busy => _hostServices.Busy;

    /// <inheritdoc />
    public IDockDialogService Dialogs => _hostServices.Dialogs;

    /// <inheritdoc />
    public IDockConfirmationService Confirmations => _hostServices.Confirmations;

    /// <inheritdoc />
    public IDockGlobalBusyService GlobalBusyService => _hostServices.GlobalBusyService;

    /// <inheritdoc />
    public IDockGlobalDialogService GlobalDialogService => _hostServices.GlobalDialogService;

    /// <inheritdoc />
    public IDockGlobalConfirmationService GlobalConfirmationService => _hostServices.GlobalConfirmationService;

    /// <summary>
    /// Clears overlay state and reload handlers.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        Busy.SetReloadHandler(null);
        Dialogs.CancelAll();
        Confirmations.CancelAll();
    }

    private async Task ReloadCurrentAsync()
    {
        var reloadable = FindReloadable(ActiveDockable);
        if (reloadable is null)
        {
            return;
        }

        await reloadable.ReloadAsync().ConfigureAwait(false);
    }

    private static IReloadable? FindReloadable(IDockable? dockable)
    {
        if (dockable is null)
        {
            return null;
        }

        if (dockable is IDock dock && dock.ActiveDockable is not null)
        {
            return FindReloadable(dock.ActiveDockable);
        }

        if (dockable is IScreen screen)
        {
            var current = screen.Router.NavigationStack.LastOrDefault();
            if (current is IReloadable reloadable)
            {
                return reloadable;
            }
        }

        return dockable as IReloadable;
    }
}
