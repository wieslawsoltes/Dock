using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Settings.UnitTests.TestAppBuilder))]

namespace Dock.Settings.UnitTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public class App : Application
{
}
