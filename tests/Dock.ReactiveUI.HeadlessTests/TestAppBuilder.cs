using Avalonia;
using Avalonia.Headless;
using ReactiveUI.Avalonia.Reactive;

[assembly: AvaloniaTestApplication(typeof(Dock.ReactiveUI.HeadlessTests.TestAppBuilder))]

namespace Dock.ReactiveUI.HeadlessTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<DockReactiveUICanonicalSample.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .UseReactiveUI(static _ => { });
}
