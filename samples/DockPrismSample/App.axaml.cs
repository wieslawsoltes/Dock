using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Avalonia.Diagnostics;
using DockPrismSample.Themes;
using DockPrismSample.ViewModels;
using DockPrismSample.Views;

namespace DockPrismSample;

public partial class App : Application
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new FluentThemeManager();
#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // DockManager.s_enableSplitToWindow = true;

        var mainWindowViewModel = new MainWindowViewModel();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktopLifetime:
            {
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

#if DEBUG
                mainWindow.AttachDockDebug(() => mainWindowViewModel.Layout, new KeyGesture(Key.F11));
                mainWindow.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif

                mainWindow.Closing += (_, _) =>
                {
                    mainWindowViewModel.CloseLayout();
                };

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (_, _) =>
                {
                    mainWindowViewModel.CloseLayout();
                };
                    
                break;
            }
            case ISingleViewApplicationLifetime singleViewLifetime:
            {
                var mainView = new MainView()
                {
                    DataContext = mainWindowViewModel
                };

                singleViewLifetime.MainView = mainView;

                break;
            }
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
