using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Windows;

namespace Dock.Avalonia.Windows;

/// <summary>
/// Default Avalonia implementations for <see cref="HostWindowFactory"/>.
/// </summary>
public static partial class HostWindowFactory
{
    /// <inheritdoc cref="Dock.Model.Windows.HostWindowFactory.CreateDefaultHostWindow"/>
    public static partial IHostWindow? CreateDefaultHostWindow()
    {
        return new HostWindow();
    }

    /// <inheritdoc cref="Dock.Model.Windows.HostWindowFactory.CreateOwnedHostWindow(object)"/>
    public static partial IHostWindow? CreateOwnedHostWindow(object owner)
    {
        return owner is Window window
            ? new HostWindow { Owner = window }
            : new HostWindow();
    }
}

