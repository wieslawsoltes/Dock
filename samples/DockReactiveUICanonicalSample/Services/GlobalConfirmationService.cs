using System;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class GlobalConfirmationService : ReactiveObject, IGlobalConfirmationService
{
    private readonly string _defaultMessage;
    private int _confirmationCount;
    private bool _isConfirmationOpen;
    private string? _message;

    public GlobalConfirmationService(string? defaultMessage = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Confirmation open in another window."
            : defaultMessage;
    }

    public bool IsConfirmationOpen
    {
        get => _isConfirmationOpen;
        private set => this.RaiseAndSetIfChanged(ref _isConfirmationOpen, value);
    }

    public string? Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _confirmationCount);
        UpdateState(true, message ?? _defaultMessage);

        return Disposable.Create(() =>
        {
            if (Interlocked.Decrement(ref _confirmationCount) <= 0)
            {
                Interlocked.Exchange(ref _confirmationCount, 0);
                UpdateState(false, null);
            }
        });
    }

    private void UpdateState(bool isOpen, string? message)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            IsConfirmationOpen = isOpen;
            Message = message;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            IsConfirmationOpen = isOpen;
            Message = message;
        });
    }
}
