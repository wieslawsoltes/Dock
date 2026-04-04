using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace DockReactiveUIRoutingSample;

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
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
