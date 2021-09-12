using System;
using System.Windows.Input;

namespace Dock.Model.Avalonia.Internal
{
    internal class Command<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public Command(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T p)
            {
                return _canExecute.Invoke(p);
            }

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T p)
            {
                _execute.Invoke(p);
            }
        }
    }
}
