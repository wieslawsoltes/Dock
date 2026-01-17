using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationRequest
{
    private readonly TaskCompletionSource<bool> _completion;
    private readonly Action<ConfirmationRequest, bool> _close;

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
        Confirm = ReactiveCommand.Create(() => _close(this, true));
        Cancel = ReactiveCommand.Create(() => _close(this, false));
    }

    public string Title { get; }

    public string Message { get; }

    public string ConfirmText { get; }

    public string CancelText { get; }

    public ReactiveCommand<Unit, Unit> Confirm { get; }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    internal IDisposable? GlobalHandle { get; set; }

    internal Task<bool> Task => _completion.Task;

    internal void Complete(bool result) => _completion.TrySetResult(result);
}
