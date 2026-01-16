using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockOverlayReactiveUISample.Themes;
using DockOverlayReactiveUISample.ViewModels;
using DockOverlayReactiveUISample.Views;

namespace DockOverlayReactiveUISample;

public class App : Application
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new FluentThemeManager();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowViewModel = new MainWindowViewModel();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();
                desktop.Exit += (_, _) => mainWindowViewModel.CloseLayout();
                desktop.MainWindow = mainWindow;
                break;
            }
            case ISingleViewApplicationLifetime singleView:
            {
                var view = new MainView
                {
                    DataContext = mainWindowViewModel
                };
                singleView.MainView = view;
                break;
            }
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
