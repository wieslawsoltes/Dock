using Avalonia;
using System;
using Dock.Model.Core;
using Dock.Settings;

namespace BrowserTabTheme;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseDefaultFloatingWindowOwnerMode(DockWindowOwnerMode.RootWindow)
            .UseFloatingWindowOwnerPolicy(DockFloatingWindowOwnerPolicy.AlwaysOwned)
            .CloseFloatingWindowsOnMainWindowClose(true)
            .LogToTrace();
}
