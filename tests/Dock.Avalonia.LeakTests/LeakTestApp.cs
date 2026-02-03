using Avalonia;
using Avalonia.Headless;

namespace Dock.Avalonia.LeakTests;

internal sealed class LeakTestsApp : Application
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<LeakTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
