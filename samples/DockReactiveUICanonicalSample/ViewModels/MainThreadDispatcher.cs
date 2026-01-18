using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

internal static class MainThreadDispatcher
{
    public static Task InvokeAsync(Action action)
    {
        var completion = new TaskCompletionSource<object?>();

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
