using System;
using System.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Default global confirmation service implementation.
/// </summary>
public sealed partial class DockGlobalConfirmationService : ReactiveObject, IDockGlobalConfirmationService
{
    private readonly string _defaultMessage;
    private readonly ServiceDispatcher _dispatcher;
    private int _confirmationCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockGlobalConfirmationService"/> class.
    /// </summary>
    /// <param name="defaultMessage">The default message to display when confirmations are open.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockGlobalConfirmationService(
        string? defaultMessage = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _defaultMessage = string.IsNullOrWhiteSpace(defaultMessage)
            ? "Confirmation open in another window."
            : defaultMessage;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
    }

    /// <inheritdoc />
    [Reactive]
    public partial bool IsConfirmationOpen { get; private set; }

    /// <inheritdoc />
    [Reactive]
    public partial string? Message { get; private set; }

    /// <inheritdoc />
    public IDisposable Begin(string? message = null)
    {
        Interlocked.Increment(ref _confirmationCount);
        UpdateState(true, message ?? _defaultMessage);

        return new ActionDisposable(() =>
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
        _dispatcher.Post(() =>
        {
            IsConfirmationOpen = isOpen;
            Message = message;
        });
    }
}
