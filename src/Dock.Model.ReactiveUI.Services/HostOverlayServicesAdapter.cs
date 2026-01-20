using System;
using Dock.Model.Services;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Adapts existing overlay services into a host overlay services instance.
/// </summary>
public sealed class HostOverlayServicesAdapter : IHostOverlayServices
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostOverlayServicesAdapter"/> class.
    /// </summary>
    /// <param name="busy">The busy service.</param>
    /// <param name="dialogs">The dialog service.</param>
    /// <param name="confirmations">The confirmation service.</param>
    /// <param name="globalBusyService">The global busy service.</param>
    /// <param name="globalDialogService">The global dialog service.</param>
    /// <param name="globalConfirmationService">The global confirmation service.</param>
    public HostOverlayServicesAdapter(
        IDockBusyService busy,
        IDockDialogService dialogs,
        IDockConfirmationService confirmations,
        IDockGlobalBusyService globalBusyService,
        IDockGlobalDialogService globalDialogService,
        IDockGlobalConfirmationService globalConfirmationService)
    {
        Busy = busy ?? throw new ArgumentNullException(nameof(busy));
        Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        Confirmations = confirmations ?? throw new ArgumentNullException(nameof(confirmations));
        GlobalBusyService = globalBusyService ?? throw new ArgumentNullException(nameof(globalBusyService));
        GlobalDialogService = globalDialogService ?? throw new ArgumentNullException(nameof(globalDialogService));
        GlobalConfirmationService = globalConfirmationService ?? throw new ArgumentNullException(nameof(globalConfirmationService));
    }

    /// <inheritdoc />
    public IDockBusyService Busy { get; }

    /// <inheritdoc />
    public IDockDialogService Dialogs { get; }

    /// <inheritdoc />
    public IDockConfirmationService Confirmations { get; }

    /// <inheritdoc />
    public IDockGlobalBusyService GlobalBusyService { get; }

    /// <inheritdoc />
    public IDockGlobalDialogService GlobalDialogService { get; }

    /// <inheritdoc />
    public IDockGlobalConfirmationService GlobalConfirmationService { get; }
}
