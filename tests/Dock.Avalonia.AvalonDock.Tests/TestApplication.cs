using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AvalonDock.Themes;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.AvalonDock.Tests.TestApplication))]

namespace Dock.Avalonia.AvalonDock.Tests;

public class TestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new AvalonDockTheme());
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
