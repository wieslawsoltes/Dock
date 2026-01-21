using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Threading;

internal sealed class ServiceDispatcher
{
    private readonly SynchronizationContext? _context;
    private readonly IScheduler _scheduler;

    public ServiceDispatcher(SynchronizationContext? context = null, IScheduler? scheduler = null)
    {
        _context = context ?? SynchronizationContext.Current;
        _scheduler = scheduler ?? RxApp.MainThreadScheduler;
    }

    public bool CheckAccess()
    {
        return _context is not null && ReferenceEquals(SynchronizationContext.Current, _context);
    }

    public void Post(Action action)
    {
        if (_context is not null)
        {
            if (CheckAccess())
            {
                action();
                return;
            }

            _context.Post(_ => action(), null);
            return;
        }

        _scheduler.Schedule(action);
    }

    public Task InvokeAsync(Action action)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Execute()
        {
            try
            {
                action();
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        if (_context is not null)
        {
            if (CheckAccess())
            {
                Execute();
                return tcs.Task;
            }

            _context.Post(_ => Execute(), null);
            return tcs.Task;
        }

        _scheduler.Schedule(Execute);

        return tcs.Task;
    }
}
