using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
#if DEBUG
using Avalonia.Diagnostics;
#endif
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Themes;
using Dock.Avalonia.Themes.Fluent;
using DockReactiveUIFlatSample.Services;
using DockReactiveUIFlatSample.ViewModels;
using DockReactiveUIFlatSample.Views;

namespace DockReactiveUIFlatSample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires unreferenced code for MainWindowViewModel.")]
public partial class App : Application
{
    public static IDockThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new DockFluentThemeManager();
#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // DockManager.s_enableSplitToWindow = true;

        var themeService = ThemeManager is null ? null : new DockThemeService(ThemeManager);
        var mainWindowViewModel = new MainWindowViewModel(themeService);

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktopLifetime:
            {
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
#if DEBUG
                mainWindow.AttachDockDebug(
                    () => mainWindowViewModel.Layout!, 
                    new KeyGesture(Key.F11));
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
