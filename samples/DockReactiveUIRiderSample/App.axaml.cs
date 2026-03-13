using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Diagnostics.Controls;
using DockReactiveUIRiderSample.ViewModels;
using DockReactiveUIRiderSample.Views;

namespace DockReactiveUIRiderSample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires unreferenced code for MainWindowViewModel.")]
public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
                mainWindow.AttachDockDebug(
                    () => mainWindowViewModel.Layout!,
                    new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F11));
                mainWindow.AttachDockDebugOverlay(new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F9));
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
                var mainView = new MainView
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
