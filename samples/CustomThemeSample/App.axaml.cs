using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CustomThemeSample.Themes;

namespace CustomThemeSample;

public partial class App : Application
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ThemeManager = new CustomThemeManager();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
