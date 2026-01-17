using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogRequest
{
    private readonly TaskCompletionSource<object?> _completion;
    private readonly Action<DialogRequest, object?> _close;

    public DialogRequest(object content, string? title, Action<DialogRequest, object?> close)
    {
        Content = content;
        Title = title;
        _close = close;
        _completion = new TaskCompletionSource<object?>();
        Close = ReactiveCommand.Create<object?>(result => _close(this, result));
    }

    public object Content { get; }

    public string? Title { get; }

    public ReactiveCommand<object?, Unit> Close { get; }

    internal IDisposable? GlobalHandle { get; set; }

    internal Task<object?> Task => _completion.Task;

    internal void Complete(object? result) => _completion.TrySetResult(result);
}
