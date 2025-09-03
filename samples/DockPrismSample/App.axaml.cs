using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using DockPrismSample.Themes;
using DockPrismSample.ViewModels;
using DockPrismSample.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Navigation.Regions;

namespace DockPrismSample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires unreferenced code for MainWindowViewModel.")]
public partial class App : PrismApplication
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new FluentThemeManager();
        AvaloniaXamlLoader.Load(this);
        
        // Required by Prism.Avalonia when overriding Initialize()
        base.Initialize();
    }

    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <summary>Register you Services, Views, Dialogs, etc.</summary>
    /// <param name="containerRegistry">container</param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Services
        //// containerRegistry.RegisterSingleton<ISerialPortService, SerialPortService>();
        containerRegistry.RegisterSingleton<IFactory, DockFactory>();
        containerRegistry.RegisterSingleton<IDockSerializer, DockSerializer>();

        // Views - Generic
        containerRegistry.Register<MainWindow>();

        // Views
        //// containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>();
    }

    /*
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

        
#if DEBUG
        this.AttachDevTools();
#endif
    }
    */
}
