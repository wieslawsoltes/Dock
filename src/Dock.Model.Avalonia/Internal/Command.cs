using System;
using System.Windows.Input;

namespace Dock.Model.Avalonia.Internal
{
    internal class Command : ICommand
    {
        public static ICommand Create(Action execute) => new Command(execute);

        public static ICommand Create(Action execute, Func<bool> canExecute) => new Command(execute, canExecute);

        public static ICommand Create<T>(Action<T> execute) => new Command<T>(execute);

        public static ICommand Create<T>(Action<T> execute, Func<T?, bool> canExecute) => new Command<T>(execute, canExecute);

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public Command(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (() => true);
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object? parameter)
        {
            _execute.Invoke();
        }
    }
}
