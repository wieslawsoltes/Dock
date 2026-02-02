// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Collections;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Dock used to host managed floating windows.
/// </summary>
public sealed class ManagedWindowDock : ManagedDockableBase, IDock
{
    private static readonly ICommand s_noOpCommand = new NoOpCommand();
    private IList<IDockable>? _visibleDockables;
    private IDockable? _activeDockable;
    private IDockable? _defaultDockable;
    private IDockable? _focusedDockable;
    private bool _isActive;
    private int _openedDockablesCount;
    private bool _canCloseLastDockable = true;
    private bool _enableGlobalDocking = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedWindowDock"/> class.
    /// </summary>
    public ManagedWindowDock()
    {
        _visibleDockables = new AvaloniaList<IDockable>();
        IsCollapsable = false;
    }

    /// <inheritdoc />
    public IList<IDockable>? VisibleDockables
    {
        get => _visibleDockables;
        set => SetProperty(ref _visibleDockables, value);
    }

    /// <inheritdoc />
    public IDockable? ActiveDockable
    {
        get => _activeDockable;
        set
        {
            if (ReferenceEquals(_activeDockable, value))
            {
                return;
            }

            var previousWindow = _activeDockable as ManagedDockWindowDocument;
            var currentWindow = value as ManagedDockWindowDocument;

            if (SetProperty(ref _activeDockable, value))
            {
                Factory?.InitActiveDockable(value, this);
                NotifyWindowActivation(previousWindow, currentWindow);
            }
        }
    }

    /// <inheritdoc />
    public IDockable? DefaultDockable
    {
        get => _defaultDockable;
        set => SetProperty(ref _defaultDockable, value);
    }

    /// <inheritdoc />
    public IDockable? FocusedDockable
    {
        get => _focusedDockable;
        set
        {
            if (SetProperty(ref _focusedDockable, value))
            {
                Factory?.OnFocusedDockableChanged(value);
            }
        }
    }

    /// <inheritdoc />
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    /// <inheritdoc />
    public int OpenedDockablesCount
    {
        get => _openedDockablesCount;
        set => SetProperty(ref _openedDockablesCount, value);
    }

    /// <inheritdoc />
    public bool CanCloseLastDockable
    {
        get => _canCloseLastDockable;
        set => SetProperty(ref _canCloseLastDockable, value);
    }

    /// <inheritdoc />
    public bool CanGoBack => false;

    /// <inheritdoc />
    public bool CanGoForward => false;

    /// <inheritdoc />
    public ICommand GoBack => s_noOpCommand;

    /// <inheritdoc />
    public ICommand GoForward => s_noOpCommand;

    /// <inheritdoc />
    public ICommand Navigate => s_noOpCommand;

    /// <inheritdoc />
    public ICommand Close => s_noOpCommand;

    /// <inheritdoc />
    public bool EnableGlobalDocking
    {
        get => _enableGlobalDocking;
        set => SetProperty(ref _enableGlobalDocking, value);
    }

    private void NotifyWindowActivation(ManagedDockWindowDocument? previous, ManagedDockWindowDocument? current)
    {
        if (Factory is null)
        {
            return;
        }

        if (previous?.Window is { } previousWindow)
        {
            Factory.OnWindowDeactivated(previousWindow);
            if (previousWindow.Layout?.ActiveDockable is { } previousDockable)
            {
                Factory.OnDockableDeactivated(previousDockable);
            }
        }

        if (current?.Window is { } currentWindow)
        {
            Factory.OnWindowActivated(currentWindow);
            if (currentWindow.Layout?.ActiveDockable is { } currentDockable)
            {
                Factory.OnDockableActivated(currentDockable);
            }
        }
    }

    /// <summary>
    /// Adds a managed dock window to the dock.
    /// </summary>
    public void AddWindow(ManagedDockWindowDocument window)
    {
        if (window is null)
        {
            return;
        }

        VisibleDockables ??= new AvaloniaList<IDockable>();

        if (!VisibleDockables.Contains(window))
        {
            window.Owner = this;
            window.Factory = Factory;
            VisibleDockables.Add(window);
            OpenedDockablesCount = VisibleDockables.Count;
            ActiveDockable = window;
        }
    }

    /// <summary>
    /// Removes a managed dock window from the dock.
    /// </summary>
    public void RemoveWindow(ManagedDockWindowDocument window)
    {
        if (VisibleDockables is null || window is null)
        {
            return;
        }

        if (VisibleDockables.Remove(window))
        {
            OpenedDockablesCount = VisibleDockables.Count;
            if (ReferenceEquals(ActiveDockable, window))
            {
                ActiveDockable = VisibleDockables.Count > 0 ? VisibleDockables[VisibleDockables.Count - 1] : null;
            }
        }
    }

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => false;

        public void Execute(object? parameter)
        {
        }
    }
}
