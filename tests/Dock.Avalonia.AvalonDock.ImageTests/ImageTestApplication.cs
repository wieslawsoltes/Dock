using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.AvalonDock.ImageTests.ImageTestApplication))]

namespace Dock.Avalonia.AvalonDock.ImageTests;

public class ImageTestApplication : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<ImageTestApplication>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
