using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model;

namespace AvaloniaDemo;

public class App : Application
{
    public static FluentTheme Fluent = new FluentTheme(new Uri("avares://ControlCatalog/Styles"))
    {
        Mode = FluentThemeMode.Light
    };

    public static SimpleTheme Simple = new SimpleTheme(new Uri("avares://ControlCatalog/Styles"))
    {
        Mode = SimpleThemeMode.Light
    };

    public static readonly Styles DockFluent = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockFluentTheme.axaml")
        }
    };

    public static readonly Styles DockSimple = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockSimpleTheme.axaml")
        }
    };

    public static readonly Styles FluentDark = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/FluentDark.axaml")
        }
    };

    public static readonly Styles FluentLight = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/FluentLight.axaml")
        }
    };

    public static readonly Styles SimpleLight = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/SimpleLight.axaml")
        }
    };

    public static readonly Styles SimpleDark = new Styles
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/SimpleDark.axaml")
        }
    };

    public override void Initialize()
    {
#if true
        Styles.Insert(0, Fluent);
        Styles.Insert(1, DockFluent);
        Styles.Insert(2, FluentLight);

        AvaloniaXamlLoader.Load(this);
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
    }
}
