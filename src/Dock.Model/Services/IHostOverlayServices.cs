namespace Dock.Model.Services;

/// <summary>
/// Provides access to per-host overlay services.
/// </summary>
public interface IHostOverlayServices
{
    /// <summary>
    /// Gets the busy service for the host.
    /// </summary>
    IDockBusyService Busy { get; }

    /// <summary>
    /// Gets the dialog service for the host.
    /// </summary>
    IDockDialogService Dialogs { get; }

    /// <summary>
    /// Gets the confirmation service for the host.
    /// </summary>
    IDockConfirmationService Confirmations { get; }

    /// <summary>
    /// Gets the global busy service.
    /// </summary>
    IDockGlobalBusyService GlobalBusyService { get; }

    /// <summary>
    /// Gets the global dialog service.
    /// </summary>
    IDockGlobalDialogService GlobalDialogService { get; }

    /// <summary>
    /// Gets the global confirmation service.
    /// </summary>
    IDockGlobalConfirmationService GlobalConfirmationService { get; }
}
