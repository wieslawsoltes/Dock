using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.MarkupExtension.UnitTests.TestAppBuilder))]

namespace Dock.MarkupExtension.UnitTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
