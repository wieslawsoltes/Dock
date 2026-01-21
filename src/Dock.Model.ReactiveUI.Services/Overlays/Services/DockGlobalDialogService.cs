using System;
using System.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Threading;

namespace Dock.Model.ReactiveUI.Services.Overlays.Services;

/// <summary>
/// Default global dialog service implementation.
/// </summary>
public sealed partial class DockGlobalDialogService : ReactiveObject, IDockGlobalDialogService
{
    private readonly string _defaultMessage;
    private readonly ServiceDispatcher _dispatcher;
    private int _dialogCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockGlobalDialogService"/> class.
    /// </summary>
    /// <param name="defaultMessage">The default message to display when dialogs are open.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockGlobalDialogService(
        string? defaultMessage = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Dialog open in another window."
            : defaultMessage;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
    }

    /// <inheritdoc />
    [Reactive]
    public partial bool IsDialogOpen { get; private set; }

    /// <inheritdoc />
    [Reactive]
    public partial string? Message { get; private set; }

    /// <inheritdoc />
    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _dialogCount);
        UpdateState(true, message ?? _defaultMessage);

        return new ActionDisposable(() =>
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
        _dispatcher.Post(() =>
        {
            IsDialogOpen = isOpen;
            Message = message;
        });
    }
}
