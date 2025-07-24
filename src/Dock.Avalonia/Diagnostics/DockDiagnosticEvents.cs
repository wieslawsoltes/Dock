using System;
using Dock.Model.Core;

namespace Dock.Avalonia.Diagnostics;

/// <summary>
/// Provides events used by diagnostics tooling.
/// </summary>
public static class DockDiagnosticEvents
{
    /// <summary>
    /// Event raised when a dockable should be selected from hit testing.
    /// </summary>
    public static event Action<IDockable?>? SelectDockableRequested;

    /// <summary>
    /// Raises the <see cref="SelectDockableRequested"/> event.
    /// </summary>
    /// <param name="dockable">The dockable to select.</param>
    internal static void RaiseSelectDockableRequested(IDockable? dockable) =>
        SelectDockableRequested?.Invoke(dockable);
}
