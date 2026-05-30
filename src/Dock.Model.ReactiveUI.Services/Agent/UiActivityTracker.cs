using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Tracks asynchronous UI work so agents can wait for deterministic quiescence before reading state.
/// </summary>
public sealed class UiActivityTracker
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, string> _operations = new();
    private readonly Func<CancellationToken, ValueTask>? _drainUiAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="UiActivityTracker"/> class.
    /// </summary>
    /// <param name="drainUiAsync">An optional host callback that drains the UI dispatcher/render queue.</param>
    public UiActivityTracker(Func<CancellationToken, ValueTask>? drainUiAsync = null)
    {
        _drainUiAsync = drainUiAsync;
    }

    /// <summary>
    /// Gets a value indicating whether tracked work is active.
    /// </summary>
    public bool IsBusy
    {
        get
        {
            lock (_gate)
            {
                return _operations.Count > 0;
            }
        }
    }

    /// <summary>
    /// Gets names of currently active operations.
    /// </summary>
    public string[] ActiveOperations
    {
        get
        {
            lock (_gate)
            {
                var names = new string[_operations.Count];
                _operations.Values.CopyTo(names, 0);
                return names;
            }
        }
    }

    /// <summary>
    /// Begins tracking a named operation.
    /// </summary>
    /// <param name="name">The operation name.</param>
    /// <returns>A disposable that ends tracking.</returns>
    public IDisposable Begin(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Operation name is required.", nameof(name));
        }

        var id = Guid.NewGuid();
        lock (_gate)
        {
            _operations.Add(id, name);
        }

        return new ActivityScope(this, id);
    }

    /// <summary>
    /// Waits until no tracked operations are active and the optional UI drain callback completes.
    /// </summary>
    /// <param name="timeout">The maximum wait duration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the UI is idle.</returns>
    public async Task WaitForIdleAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsBusy)
            {
                if (_drainUiAsync is not null)
                {
                    await _drainUiAsync(cancellationToken).ConfigureAwait(false);
                }

                if (!IsBusy)
                {
                    return;
                }
            }

            if (stopwatch.Elapsed > timeout)
            {
                throw new TimeoutException($"UI did not become idle. Active: {string.Join(", ", ActiveOperations)}");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken).ConfigureAwait(false);
        }
    }

    private void End(Guid id)
    {
        lock (_gate)
        {
            _operations.Remove(id);
        }
    }

    private sealed class ActivityScope : IDisposable
    {
        private readonly UiActivityTracker _owner;
        private readonly Guid _id;
        private bool _disposed;

        public ActivityScope(UiActivityTracker owner, Guid id)
        {
            _owner = owner;
            _id = id;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.End(_id);
        }
    }
}
