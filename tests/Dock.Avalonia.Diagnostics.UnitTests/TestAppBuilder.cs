using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.Diagnostics.UnitTests.TestAppBuilder))]

namespace Dock.Avalonia.Diagnostics.UnitTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
