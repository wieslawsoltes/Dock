using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class BusyService : ReactiveObject, IBusyService
{
    private readonly IGlobalBusyService? _globalBusyService;
    private int _busyCount;
    private bool _isBusy;
    private string? _message;
    private bool _isReloadVisible = true;
    private Func<Task>? _reloadHandler;

    private readonly ReactiveCommand<Unit, Unit> _reloadCommand;

    public BusyService(IGlobalBusyService? globalBusyService = null)
    {
        _globalBusyService = globalBusyService;
        var canReload = this.WhenAnyValue(x => x.CanReload);
        _reloadCommand = ReactiveCommand.CreateFromTask(
            ExecuteReloadAsync,
            canReload);
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

    public bool IsReloadVisible
    {
        get => _isReloadVisible;
        set => this.RaiseAndSetIfChanged(ref _isReloadVisible, value);
    }

    public bool CanReload => _reloadHandler is not null;

    public ICommand ReloadCommand => _reloadCommand;

    public IDisposable Begin(string message)
    {
        var globalHandle = _globalBusyService?.Begin();
        Interlocked.Increment(ref _busyCount);
        UpdateState(true, message);

        return Disposable.Create(() =>
        {
            if (Interlocked.Decrement(ref _busyCount) <= 0)
            {
                Interlocked.Exchange(ref _busyCount, 0);
                UpdateState(false, null);
            }

            globalHandle?.Dispose();
        });
    }

    public async Task RunAsync(string message, Func<Task> action)
    {
        using var handle = Begin(message);
        await action().ConfigureAwait(false);
    }

    public void UpdateMessage(string? message)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Message = message;
            return;
        }

        Dispatcher.UIThread.Post(() => Message = message);
    }

    public void SetReloadHandler(Func<Task>? handler)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            SetReloadHandlerCore(handler);
            return;
        }

        Dispatcher.UIThread.Post(() => SetReloadHandlerCore(handler));
    }

    private void SetReloadHandlerCore(Func<Task>? handler)
    {
        if (_reloadHandler == handler)
        {
            return;
        }

        _reloadHandler = handler;
        this.RaisePropertyChanged(nameof(CanReload));
    }

    private Task ExecuteReloadAsync()
    {
        var handler = _reloadHandler;
        if (handler is null)
        {
            return Task.CompletedTask;
        }

        return handler();
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
