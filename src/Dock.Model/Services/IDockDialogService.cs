using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Dock.Model.Services;

/// <summary>
/// Manages dialog requests for a host window.
/// </summary>
public interface IDockDialogService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the active dialog requests.
    /// </summary>
    ReadOnlyObservableCollection<DialogRequest> Dialogs { get; }

    /// <summary>
    /// Gets the currently active dialog request.
    /// </summary>
    DialogRequest? ActiveDialog { get; }

    /// <summary>
    /// Gets a value indicating whether any dialogs are active.
    /// </summary>
    bool HasDialogs { get; }

    /// <summary>
    /// Shows a dialog with the provided content.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="content">The dialog content.</param>
    /// <param name="title">The optional dialog title.</param>
    /// <returns>The dialog result.</returns>
    Task<T?> ShowAsync<T>(object content, string? title = null);

    /// <summary>
    /// Closes the specified dialog request.
    /// </summary>
    /// <param name="request">The dialog request to close.</param>
    /// <param name="result">The optional dialog result.</param>
    void Close(DialogRequest request, object? result = null);

    /// <summary>
    /// Cancels all active dialogs.
    /// </summary>
    void CancelAll();
}
