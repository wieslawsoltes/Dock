using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Dock.Avalonia.Diagnostics;

/// <summary>
/// Extension methods to attach the dock debug overlay.
/// </summary>
public static class DockDebugOverlayExtensions
{
    /// <summary>
    /// Attaches a debug overlay to all <see cref="DockControl"/> instances in the given top level.
    /// </summary>
    /// <param name="topLevel">The visual root.</param>
    /// <returns>An <see cref="IDisposable"/> that removes the overlay when disposed.</returns>
    public static IDisposable AttachDockDebugOverlay(this TopLevel topLevel)
    {
        return new DockDebugOverlayManager(topLevel);
    }

    /// <summary>
    /// Attaches a debug overlay to each window created by the application.
    /// </summary>
    /// <param name="app">The Avalonia application.</param>
    public static void AttachDockDebugOverlay(this Application app)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.WindowCreated += (_, args) => args.Window.AttachDockDebugOverlay();
            foreach (var window in desktop.Windows)
            {
                window.AttachDockDebugOverlay();
            }
        }
        else if (app.ApplicationLifetime is ISingleViewApplicationLifetime single && single.MainView is TopLevel tl)
        {
            tl.AttachDockDebugOverlay();
        }
    }
}
