using System.Windows.Input;

namespace Dock.Model.CaliburMicro.Core;

/// <summary>
/// Simple relay command implementation for Caliburn.Micro.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly System.Action _execute;
    private readonly System.Func<bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The function to determine if the command can execute.</param>
    public RelayCommand(System.Action execute, System.Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event System.EventHandler? CanExecuteChanged;

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object? parameter) => _execute();

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
}

/// <summary>
/// Generic relay command implementation for Caliburn.Micro.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly System.Action<T?> _execute;
    private readonly System.Func<T?, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The function to determine if the command can execute.</param>
    public RelayCommand(System.Action<T?> execute, System.Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event System.EventHandler? CanExecuteChanged;

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object? parameter) => _execute((T?)parameter);

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
}
