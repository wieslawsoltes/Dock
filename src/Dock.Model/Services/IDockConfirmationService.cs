using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Dock.Model.Services;

/// <summary>
/// Manages confirmation requests for a host window.
/// </summary>
public interface IDockConfirmationService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the active confirmation requests.
    /// </summary>
    ReadOnlyObservableCollection<ConfirmationRequest> Confirmations { get; }

    /// <summary>
    /// Gets the currently active confirmation request.
    /// </summary>
    ConfirmationRequest? ActiveConfirmation { get; }

    /// <summary>
    /// Gets a value indicating whether any confirmations are active.
    /// </summary>
    bool HasConfirmations { get; }

    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="confirmText">The confirm button label.</param>
    /// <param name="cancelText">The cancel button label.</param>
    /// <returns>True when confirmed; otherwise false.</returns>
    Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel");

    /// <summary>
    /// Closes the specified confirmation request.
    /// </summary>
    /// <param name="request">The confirmation request.</param>
    /// <param name="result">The confirmation result.</param>
    void Close(ConfirmationRequest request, bool result);

    /// <summary>
    /// Cancels all active confirmations.
    /// </summary>
    void CancelAll();
}
