using System;
using Dock.Model.ReactiveUI.Navigation.Services;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Controls;

/// <summary>
/// Base tool that provides overlay service helpers and navigation utilities.
/// </summary>
public abstract class RoutableToolBase : RoutableTool
{
    private readonly IHostOverlayServicesProvider? _overlayServicesProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutableToolBase"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="url">Optional url segment.</param>
    protected RoutableToolBase(IScreen host, string? url = null)
        : this(host, null, url)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutableToolBase"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="overlayServicesProvider">The overlay services provider.</param>
    /// <param name="url">Optional url segment.</param>
    protected RoutableToolBase(
        IScreen host,
        IHostOverlayServicesProvider? overlayServicesProvider,
        string? url = null)
        : base(host, url)
    {
        _overlayServicesProvider = overlayServicesProvider;
    }

    /// <summary>
    /// Gets the busy service for this tool.
    /// </summary>
    /// <returns>The busy service.</returns>
    protected IDockBusyService GetBusyService()
        => GetOverlayServices().Busy;

    /// <summary>
    /// Gets the dialog service for this tool.
    /// </summary>
    /// <returns>The dialog service.</returns>
    protected IDockDialogService GetDialogService()
        => GetOverlayServices().Dialogs;

    /// <summary>
    /// Gets the confirmation service for this tool.
    /// </summary>
    /// <returns>The confirmation service.</returns>
    protected IDockConfirmationService GetConfirmationService()
        => GetOverlayServices().Confirmations;

    /// <summary>
    /// Navigates back using the tool router.
    /// </summary>
    /// <returns>True when navigation was executed.</returns>
    protected bool NavigateBack()
        => DockNavigationHelpers.TryNavigateBack(this);

    /// <summary>
    /// Closes the tool by resolving the owning factory.
    /// </summary>
    /// <returns>True when the tool was closed.</returns>
    protected bool CloseDockable()
        => DockNavigationHelpers.TryCloseDockable(this);

    private IHostOverlayServices GetOverlayServices()
    {
        if (_overlayServicesProvider is null)
        {
            throw new InvalidOperationException("Overlay services provider is not available.");
        }

        return _overlayServicesProvider.GetServices(this);
    }
}
