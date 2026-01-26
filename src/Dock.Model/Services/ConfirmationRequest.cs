using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dock.Model.Services;

/// <summary>
/// Represents a request to show a confirmation dialog.
/// </summary>
public sealed class ConfirmationRequest
{
    private readonly TaskCompletionSource<bool> _completion;
    private readonly Action<ConfirmationRequest, bool> _close;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationRequest"/> class.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="confirmText">The confirm button label.</param>
    /// <param name="cancelText">The cancel button label.</param>
    /// <param name="close">The close callback.</param>
    public ConfirmationRequest(
        string title,
        string message,
        string confirmText,
        string cancelText,
        Action<ConfirmationRequest, bool> close)
    {
        Title = title;
        Message = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
        _close = close;
        _completion = new TaskCompletionSource<bool>();
        Confirm = new DelegateCommand(_ => _close(this, true));
        Cancel = new DelegateCommand(_ => _close(this, false));
    }

    /// <summary>
    /// Gets the dialog title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the dialog message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the confirm button label.
    /// </summary>
    public string ConfirmText { get; }

    /// <summary>
    /// Gets the cancel button label.
    /// </summary>
    public string CancelText { get; }

    /// <summary>
    /// Gets the command that confirms the request.
    /// </summary>
    public ICommand Confirm { get; }

    /// <summary>
    /// Gets the command that cancels the request.
    /// </summary>
    public ICommand Cancel { get; }

    /// <summary>
    /// Gets or sets the global confirmation handle associated with this request.
    /// </summary>
    public IDisposable? GlobalHandle { get; set; }

    /// <summary>
    /// Gets the task that completes when the confirmation closes.
    /// </summary>
    public Task<bool> Task => _completion.Task;

    /// <summary>
    /// Completes the request with the provided result.
    /// </summary>
    /// <param name="result">The confirmation result.</param>
    public void Complete(bool result) => _completion.TrySetResult(result);
}
