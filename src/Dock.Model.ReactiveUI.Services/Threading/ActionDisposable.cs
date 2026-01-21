using System;
using System.Threading;

namespace Dock.Model.ReactiveUI.Services.Threading;

internal sealed class ActionDisposable : IDisposable
{
    private Action? _dispose;

    public ActionDisposable(Action dispose)
    {
        _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
    }

    public void Dispose()
    {
        var dispose = Interlocked.Exchange(ref _dispose, null);
        dispose?.Invoke();
    }
}
