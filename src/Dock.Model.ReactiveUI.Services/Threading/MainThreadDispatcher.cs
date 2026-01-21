using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Threading;

/// <summary>
/// Default dispatcher implementation using <see cref="RxApp.MainThreadScheduler"/>.
/// </summary>
public sealed class MainThreadDispatcher : IDockDispatcher
{
    /// <inheritdoc />
    public Task InvokeAsync(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        RxApp.MainThreadScheduler.Schedule(Unit.Default, (_, __) =>
        {
            try
            {
                action();
                completion.TrySetResult(null);
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }

            return Disposable.Empty;
        });

        return completion.Task;
    }
}
