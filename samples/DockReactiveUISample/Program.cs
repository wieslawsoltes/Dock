using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.ReactiveUI;
using Dock.Settings;

namespace DockReactiveUISample;

[RequiresUnreferencedCode("Requires unreferenced code for App.")]
[RequiresDynamicCode("Requires unreferenced code for App.")]
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
            .UseReactiveUI()
            .LogToTrace();
}
