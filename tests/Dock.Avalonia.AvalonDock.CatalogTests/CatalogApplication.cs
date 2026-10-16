using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AvalonDock.Themes;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.AvalonDock.CatalogTests.CatalogApplication))]

namespace Dock.Avalonia.AvalonDock.CatalogTests;

public partial class CatalogApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new AvalonDockTheme());
    }
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<CatalogApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
