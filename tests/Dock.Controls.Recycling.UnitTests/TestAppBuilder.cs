using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Controls.Recycling.UnitTests.TestAppBuilder))]

namespace Dock.Controls.Recycling.UnitTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
