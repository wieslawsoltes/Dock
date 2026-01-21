using System;
using System.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Threading;

namespace Dock.Model.ReactiveUI.Services.Overlays.Services;

/// <summary>
/// Default global busy service implementation.
/// </summary>
public sealed partial class DockGlobalBusyService : ReactiveObject, IDockGlobalBusyService
{
    private readonly string _defaultMessage;
    private readonly ServiceDispatcher _dispatcher;
    private int _busyCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockGlobalBusyService"/> class.
    /// </summary>
    /// <param name="defaultMessage">The default message to display when busy.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockGlobalBusyService(
        string? defaultMessage = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Working..."
            : defaultMessage;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
    }

    /// <inheritdoc />
    [Reactive]
    public partial bool IsBusy { get; private set; }

    /// <inheritdoc />
    [Reactive]
    public partial string? Message { get; private set; }

    /// <inheritdoc />
    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _busyCount);
        UpdateState(true, message ?? _defaultMessage);

        return new ActionDisposable(() =>
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
        _dispatcher.Post(() =>
        {
            IsBusy = isBusy;
            Message = message;
        });
    }
}
