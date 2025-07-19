using System;
using Avalonia;
using Dock.Settings;

namespace DockChromeSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        // DockSettings.UseFloatingDockAdorner = true;
        // DockSettings.EnableGlobalDocking = true;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
