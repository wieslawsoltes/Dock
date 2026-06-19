using System;
using Avalonia;
using Dock.Model.Avalonia;

namespace DockQuickStartSample;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(Factory).Assembly);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
