// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Avalonia.Commands;

internal sealed class DockableCommand : ICommand
{
    private readonly Action<IFactory, IDockable> _execute;

    public DockableCommand(Action<IFactory, IDockable> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        return parameter is IDockable dockable && ResolveFactory(dockable) is not null;
    }

    public void Execute(object? parameter)
    {
        if (parameter is not IDockable dockable || ResolveFactory(dockable) is not { } factory)
        {
            return;
        }

        _execute(factory, dockable);
    }

    private static IFactory? ResolveFactory(IDockable dockable)
    {
        return dockable.Owner?.Factory ?? dockable.Factory;
    }
}
