using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model;

namespace AvaloniaDemo;

public class App : Application
{       
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
        },
    };

    public override void Initialize()
    {
        Styles.Insert(0, FluentLight);

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
