using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace DockFigmaSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();
}
