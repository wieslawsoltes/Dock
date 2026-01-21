using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dock.Model.Services;

/// <summary>
/// Represents a request to show a dialog.
/// </summary>
public sealed class DialogRequest
{
    private readonly TaskCompletionSource<object?> _completion;
    private readonly Action<DialogRequest, object?> _close;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogRequest"/> class.
    /// </summary>
    /// <param name="content">The dialog content.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="close">The close callback.</param>
    public DialogRequest(object content, string? title, Action<DialogRequest, object?> close)
    {
        Content = content;
        Title = title;
        _close = close;
        _completion = new TaskCompletionSource<object?>();
        Close = new DelegateCommand(result => _close(this, result));
    }

    /// <summary>
    /// Gets the dialog content.
    /// </summary>
    public object Content { get; }

    /// <summary>
    /// Gets the dialog title.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the command that closes the dialog.
    /// </summary>
    public ICommand Close { get; }

    /// <summary>
    /// Gets or sets the global dialog handle associated with this request.
    /// </summary>
    public IDisposable? GlobalHandle { get; set; }

    /// <summary>
    /// Gets the task that completes when the dialog closes.
    /// </summary>
    public Task<object?> Task => _completion.Task;

    /// <summary>
    /// Completes the request with the provided result.
    /// </summary>
    /// <param name="result">The dialog result.</param>
    public void Complete(object? result) => _completion.TrySetResult(result);
}
