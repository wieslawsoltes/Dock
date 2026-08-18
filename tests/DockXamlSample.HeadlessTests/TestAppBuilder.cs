using Avalonia;
using Avalonia.Headless;
using DockXamlSample;

[assembly: AvaloniaTestApplication(typeof(DockXamlSample.HeadlessTests.TestAppBuilder))]

namespace DockXamlSample.HeadlessTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
