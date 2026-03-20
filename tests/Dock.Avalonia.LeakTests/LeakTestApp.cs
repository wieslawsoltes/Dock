using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Themes.Fluent;

namespace Dock.Avalonia.LeakTests;

internal sealed class LeakTestsApp : Application
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<LeakTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "Default"
            });

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
