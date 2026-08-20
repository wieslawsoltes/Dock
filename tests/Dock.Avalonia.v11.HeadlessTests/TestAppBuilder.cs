using Avalonia;
using Avalonia.Headless;
using ReactiveUI.Avalonia;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.v11.HeadlessTests.TestAppBuilder))]

namespace Dock.Avalonia.v11.HeadlessTests;

public sealed class TestApplication : Application
{
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .UseReactiveUI(static _ => { });
}
