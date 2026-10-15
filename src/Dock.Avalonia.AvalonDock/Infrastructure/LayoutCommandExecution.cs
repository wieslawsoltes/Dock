using System;
using System.Reactive.Concurrency;
using System.Runtime.ExceptionServices;
using ReactiveUI.Reactive;

namespace AvalonDock.Infrastructure;

// Keep ReactiveUI execution while preserving the synchronous ICommand error
// contract. A result belongs to one invocation, including reentrant calls.
internal sealed class LayoutCommandExecution<TParameter> : IDisposable
{
    private Action<TParameter>? _execute;
    private readonly ReactiveCommand<TParameter, ExceptionDispatchInfo?> _command;

    internal LayoutCommandExecution(Action<TParameter> execute)
    {
        _execute = execute;
        _command = ReactiveCommand.Create<TParameter, ExceptionDispatchInfo?>(Invoke,
            outputScheduler: ImmediateScheduler.Instance);
    }

    private ExceptionDispatchInfo? Invoke(TParameter parameter)
    {
        try { _execute?.Invoke(parameter); return null; }
        catch (Exception error) { return ExceptionDispatchInfo.Capture(error); }
    }

    internal void Execute(TParameter parameter)
    {
        if (_execute is null) return;
        ExceptionDispatchInfo? failure = null;
        using (_command.Execute(parameter).Subscribe(result => failure = result)) { }
        failure?.Throw();
    }

    public void Dispose()
    {
        _execute = null;
        _command.Dispose();
    }
}
