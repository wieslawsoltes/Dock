using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace DockXamlSample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = new MainWindow();
        }

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleLifetime)
        {
            singleLifetime.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
