using System;
using Avalonia;
using Dock.Settings;

namespace DockMvvmSample;

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
            .WithInterFont()
            .LogToTrace()
            .With(new MacOSPlatformOptions 
            { 
                ShowInDock = true,
                DisableDefaultApplicationMenuItems = false,
                DisableNativeMenus = false
            });
}
