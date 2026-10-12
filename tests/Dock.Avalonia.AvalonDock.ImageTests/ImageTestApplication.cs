using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.AvalonDock.ImageTests.ImageTestApplication))]

namespace Dock.Avalonia.AvalonDock.ImageTests;

public class ImageTestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new global::Avalonia.Themes.Fluent.FluentTheme());
        Styles.Add(new global::AvalonDock.Themes.AvalonDockTheme());
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<ImageTestApplication>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
