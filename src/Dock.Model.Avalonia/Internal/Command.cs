/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Windows.Input;

namespace Dock.Model.Avalonia.Internal;

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
