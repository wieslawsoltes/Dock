using System;
#if DOCK_REACTIVEUI_REACTIVE
using System.Reactive.Concurrency;
#else
using ReactiveUI.Primitives.Concurrency;
#endif
using System.Threading.Tasks;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Threading;

/// <summary>
/// Default dispatcher implementation using <see cref="RxSchedulers.MainThreadScheduler"/>.
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

        RxSchedulers.MainThreadScheduler.Schedule(() =>
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
        });

        return completion.Task;
    }
}
