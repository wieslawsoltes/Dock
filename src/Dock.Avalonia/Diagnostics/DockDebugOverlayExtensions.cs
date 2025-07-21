// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics;

/// <summary>
/// Extension methods to attach the dock debug overlay.
/// </summary>
public static class DockDebugOverlayExtensions
{
    /// <summary>
    /// Attaches a debug overlay to all <see cref="DockControl"/> instances in the
    /// given top level and toggles it with <paramref name="gesture"/>.
    /// </summary>
    /// <param name="topLevel">The visual root.</param>
    /// <param name="gesture">The key gesture used to toggle the overlay.</param>
    /// <returns>An <see cref="IDisposable"/> that unregisters the hotkey and
    /// removes the overlay when disposed.</returns>
    public static IDisposable AttachDockDebugOverlay(
        this TopLevel topLevel,
        KeyGesture? gesture = null)
    {
        var keyGesture = gesture ?? new KeyGesture(Key.F9);
        DockDebugOverlayManager? manager = null;

        void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (!keyGesture.Matches(e))
            {
                return;
            }

            if (manager is null)
            {
                manager = new DockDebugOverlayManager(topLevel);
            }
            else
            {
                manager.Dispose();
                manager = null;
            }
        }

        topLevel.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        return new ActionDisposable(() =>
        {
            topLevel.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            manager?.Dispose();
            manager = null;
        });
    }

    /// <summary>
    /// Attaches a debug overlay to each window created by the application.
    /// </summary>
    /// <param name="app">The Avalonia application.</param>
    /// <param name="gesture">The key gesture used to toggle the overlay.</param>
    public static void AttachDockDebugOverlay(this Application app, KeyGesture? gesture = null)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                window.AttachDockDebugOverlay(gesture);
            }
        }
        else if (app.ApplicationLifetime is ISingleViewApplicationLifetime single && single.MainView is TopLevel tl)
        {
            tl.AttachDockDebugOverlay(gesture);
        }
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
