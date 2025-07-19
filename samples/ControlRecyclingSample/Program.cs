using System;
using Avalonia;
using Dock.Model.Avalonia;

namespace ControlRecyclingSample;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(Factory).Assembly);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
