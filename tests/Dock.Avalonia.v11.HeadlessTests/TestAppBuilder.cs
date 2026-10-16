using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using ReactiveUI.Avalonia;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.v11.HeadlessTests.TestAppBuilder))]

namespace Dock.Avalonia.v11.HeadlessTests;

public sealed class TestApplication : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .UseReactiveUI(static _ => { });
}
