// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Provides extension method to attach a <see cref="RootDockDebug"/> window.
/// </summary>
public static class RootDockDebugExtensions
{
    /// <summary>
    /// Attaches <see cref="RootDockDebug"/> toggled by <paramref name="gesture"/>.
    /// </summary>
    /// <param name="topLevel">The top level to attach to.</param>
    /// <param name="root">The root dock debug context.</param>
    /// <param name="gesture">The key gesture used to toggle debug.</param>
    /// <returns>IDisposable instance detaching the debug window.</returns>
    public static IDisposable AttachDockDebug(
        this TopLevel topLevel,
        object root,
        KeyGesture? gesture = null)
    {
        var keyGesture = gesture ?? new KeyGesture(Key.F11);
        Window? debugWindow = null;

        void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (keyGesture.Matches(e))
            {
                if (debugWindow is null)
                {
                    debugWindow = new Window
                    {
                        Width = 400,
                        Height = 680,
                        Content = new RootDockDebug { DataContext = root }
                    };
                    debugWindow.Closed += (_, _) => debugWindow = null;
                    debugWindow.Show();
                }
                else
                {
                    debugWindow.Close();
                }
            }
        }

        void OnMainWindowClosing(object? sender, EventArgs e)
        {
            if (debugWindow is { })
            {
                debugWindow.Close();
                debugWindow = null;
            }
        }

        topLevel.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        
        // Listen for the main window closing event
        if (topLevel is Window mainWindow)
        {
            mainWindow.Closing += OnMainWindowClosing;
        }

        return new ActionDisposable(() =>
        {
            topLevel.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            
            // Remove the main window closing event handler
            if (topLevel is Window mainWindow)
            {
                mainWindow.Closing -= OnMainWindowClosing;
            }
            
            if (debugWindow is { })
            {
                debugWindow.Close();
                debugWindow = null;
            }
        });
    }

    private sealed class ActionDisposable : IDisposable
    {
        private readonly Action _dispose;

        public ActionDisposable(Action dispose) => _dispose = dispose;

        /// <summary>
        /// Executes the stored dispose action.
        /// </summary>
        public void Dispose() => _dispose();
    }
}
