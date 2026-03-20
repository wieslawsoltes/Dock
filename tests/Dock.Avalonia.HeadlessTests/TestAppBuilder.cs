using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.HeadlessTests.TestAppBuilder))]

namespace Dock.Avalonia.HeadlessTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
