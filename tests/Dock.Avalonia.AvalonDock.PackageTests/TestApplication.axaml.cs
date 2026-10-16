using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(Dock.Avalonia.AvalonDock.PackageTests.TestApplication))]

namespace Dock.Avalonia.AvalonDock.PackageTests;

public partial class TestApplication : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
