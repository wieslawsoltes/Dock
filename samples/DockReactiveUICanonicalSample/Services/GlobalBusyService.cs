using System;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class GlobalBusyService : ReactiveObject, IGlobalBusyService
{
    private readonly string _defaultMessage;
    private int _busyCount;
    private bool _isBusy;
    private string? _message;

    public GlobalBusyService(string? defaultMessage = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Working..."
            : defaultMessage;
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string? Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _busyCount);
        UpdateState(true, message ?? _defaultMessage);

        return Disposable.Create(() =>
        {
            if (Interlocked.Decrement(ref _busyCount) <= 0)
            {
                Interlocked.Exchange(ref _busyCount, 0);
                UpdateState(false, null);
            }
        });
    }

    private void UpdateState(bool isBusy, string? message)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            IsBusy = isBusy;
            Message = message;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            IsBusy = isBusy;
            Message = message;
        });
    }
}
