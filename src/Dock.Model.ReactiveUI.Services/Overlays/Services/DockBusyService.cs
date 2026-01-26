using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Threading;

namespace Dock.Model.ReactiveUI.Services.Overlays.Services;

/// <summary>
/// Default busy service implementation.
/// </summary>
public sealed partial class DockBusyService : ReactiveObject, IDockBusyService
{
    private readonly IDockGlobalBusyService? _globalBusyService;
    private readonly ServiceDispatcher _dispatcher;
    private readonly ReactiveCommand<Unit, Unit> _reloadCommand;
    private int _busyCount;
    private Func<Task>? _reloadHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockBusyService"/> class.
    /// </summary>
    /// <param name="globalBusyService">The optional global busy service.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockBusyService(
        IDockGlobalBusyService? globalBusyService = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _globalBusyService = globalBusyService;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
        IsReloadVisible = true;
        var canReload = this.WhenAnyValue(x => x.CanReload);
        _reloadCommand = ReactiveCommand.CreateFromTask(ExecuteReloadAsync, canReload);
    }

    /// <inheritdoc />
    [Reactive]
    public partial bool IsBusy { get; private set; }

    /// <inheritdoc />
    [Reactive]
    public partial string? Message { get; private set; }

    /// <inheritdoc />
    [Reactive]
    public partial bool IsReloadVisible { get; set; }

    /// <inheritdoc />
    [Reactive]
    public partial bool CanReload { get; private set; }

    /// <inheritdoc />
    public ICommand ReloadCommand => _reloadCommand;

    /// <inheritdoc />
    public IDisposable Begin(string message)
    {
        var globalHandle = _globalBusyService?.Begin();
        Interlocked.Increment(ref _busyCount);
        UpdateState(true, message);

        return new ActionDisposable(() =>
        {
            if (Interlocked.Decrement(ref _busyCount) <= 0)
            {
                Interlocked.Exchange(ref _busyCount, 0);
                UpdateState(false, null);
            }

            globalHandle?.Dispose();
        });
    }

    /// <inheritdoc />
    public async Task RunAsync(string message, Func<Task> action)
    {
        using var handle = Begin(message);
        await action().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void UpdateMessage(string? message)
    {
        _dispatcher.Post(() => Message = message);
    }

    /// <inheritdoc />
    public void SetReloadHandler(Func<Task>? handler)
    {
        if (_dispatcher.CheckAccess())
        {
            SetReloadHandlerCore(handler);
            return;
        }

        _dispatcher.Post(() => SetReloadHandlerCore(handler));
    }

    private void SetReloadHandlerCore(Func<Task>? handler)
    {
        if (_reloadHandler == handler)
        {
            return;
        }

        _reloadHandler = handler;
        CanReload = _reloadHandler is not null;
    }

    private void UpdateState(bool isBusy, string? message)
    {
        _dispatcher.Post(() =>
        {
            IsBusy = isBusy;
            Message = message;
        });
    }

    private async Task ExecuteReloadAsync()
    {
        var handler = _reloadHandler;
        if (handler is null)
        {
            return;
        }

        await handler().ConfigureAwait(false);
    }

}
