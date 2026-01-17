using System;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class GlobalDialogService : ReactiveObject, IGlobalDialogService
{
    private readonly string _defaultMessage;
    private int _dialogCount;
    private bool _isDialogOpen;
    private string? _message;

    public GlobalDialogService(string? defaultMessage = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Dialog open in another window."
            : defaultMessage;
    }

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        private set => this.RaiseAndSetIfChanged(ref _isDialogOpen, value);
    }

    public string? Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _dialogCount);
        UpdateState(true, message ?? _defaultMessage);

        return Disposable.Create(() =>
        {
            if (Interlocked.Decrement(ref _dialogCount) <= 0)
            {
                Interlocked.Exchange(ref _dialogCount, 0);
                UpdateState(false, null);
            }
        });
    }

    private void UpdateState(bool isOpen, string? message)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            IsDialogOpen = isOpen;
            Message = message;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            IsDialogOpen = isOpen;
            Message = message;
        });
    }
}
