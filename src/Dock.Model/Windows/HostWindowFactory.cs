using Dock.Model.Core;

namespace Dock.Model.Windows;

/// <summary>
/// Provides helper methods for creating host windows.
/// </summary>
public static partial class HostWindowFactory
{
    /// <summary>
    /// Creates the default host window for the current platform.
    /// </summary>
    /// <returns>The new <see cref="IHostWindow"/> instance or <c>null</c>.</returns>
    public static partial IHostWindow? CreateDefaultHostWindow();

    /// <summary>
    /// Creates a host window owned by the specified owner object.
    /// </summary>
    /// <param name="owner">The owner object.</param>
    /// <returns>The new <see cref="IHostWindow"/> instance or <c>null</c>.</returns>
    public static partial IHostWindow? CreateOwnedHostWindow(object owner);
}

