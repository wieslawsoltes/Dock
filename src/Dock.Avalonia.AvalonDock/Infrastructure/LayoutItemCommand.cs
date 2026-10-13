using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows.Input;
using ReactiveUI.Reactive;

namespace AvalonDock.Infrastructure;

// ReactiveUI executes the action; the public ICommand predicate is evaluated
// on demand, including predicates supplied by a derived item before attachment.
internal sealed class LayoutItemCommand : ICommand, IDisposable
{
    private Func<bool>? _canExecute;
    private Action? _execute;
    private readonly ReactiveCommand<Unit, Unit> _command;
    private bool? _enabled;
    private bool _disposed;

    public LayoutItemCommand(Action execute, Func<bool> canExecute)
    {
        _canExecute = canExecute;
        _execute = execute;
        _command = ReactiveCommand.Create(() =>
        {
            if (CanExecute(null)) _execute?.Invoke();
        }, outputScheduler: ImmediateScheduler.Instance);
    }

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_disposed && _canExecute?.Invoke() == true;
    public void Execute(object? parameter)
    {
        if (!_disposed) ((ICommand)_command).Execute(parameter);
    }

    public void Refresh()
    {
        if (_disposed) return;
        bool enabled = CanExecute(null);
        if (_enabled == enabled) return;
        _enabled = enabled;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _canExecute = null;
        _execute = null;
        _command.Dispose();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        CanExecuteChanged = null;
    }
}
