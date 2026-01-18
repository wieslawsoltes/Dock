using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services;

internal sealed class ServiceDispatcher
{
    private readonly SynchronizationContext? _context;

    public ServiceDispatcher(SynchronizationContext? context = null)
    {
        _context = context ?? SynchronizationContext.Current;
    }

    public bool CheckAccess()
    {
        return _context is null || ReferenceEquals(SynchronizationContext.Current, _context);
    }

    public void Post(Action action)
    {
        if (CheckAccess())
        {
            action();
            return;
        }

        _context!.Post(_ => action(), null);
    }

    public Task InvokeAsync(Action action)
    {
        if (CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _context!.Post(_ =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }
}
