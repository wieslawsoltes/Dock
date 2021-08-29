using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model;
using Dock.Model.ReactiveUI.Controls;

namespace AvaloniaDemo
{
    public class App : Application
    {       
        public static readonly Styles FluentDark = new Styles
        {
            new StyleInclude(new Uri("avares://AvaloniaDemo/Styles"))
            {
                Source = new Uri("avares://AvaloniaDemo/Themes/FluentDark.axaml")
            }
        };

        public static readonly Styles FluentLight = new Styles
        {
            new StyleInclude(new Uri("avares://AvaloniaDemo/Styles"))
            {
                Source = new Uri("avares://AvaloniaDemo/Themes/FluentLight.axaml")
            }
        };

        public static readonly Styles DefaultLight = new Styles
        {
            new StyleInclude(new Uri("avares://AvaloniaDemo/Styles"))
            {
                Source = new Uri("avares://AvaloniaDemo/Themes/DefaultLight.axaml")
            }
        };

        public static readonly Styles DefaultDark = new Styles
        {
            new StyleInclude(new Uri("avares://AvaloniaDemo/Styles"))
            {
                Source = new Uri("avares://AvaloniaDemo/Themes/DefaultDark.axaml")
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

            var layout = default(RootDock);
            var layoutPath = "AvaloniaDemo.layout";

            try
            {
                if (File.Exists(layoutPath))
                {
                    var layoutJson = File.ReadAllText(layoutPath);
                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        layout = JsonSerializer.Deserialize<RootDock>(layoutJson, 
                            new JsonSerializerOptions()
                            {
                                ReferenceHandler = ReferenceHandler.Preserve
                            });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var mainWindowViewModel = new MainWindowViewModel(layout);

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

                        if (mainWindowViewModel.Layout is RootDock rootDock)
                        {
                            try
                            {
                                var layoutJson = JsonSerializer.Serialize<RootDock>(rootDock,
                                    new JsonSerializerOptions()
                                    {
                                        WriteIndented = true, 
                                        ReferenceHandler = ReferenceHandler.Preserve,
                                        NumberHandling = JsonNumberHandling.WriteAsString
                                    });
                                if (!string.IsNullOrEmpty(layoutJson))
                                {
                                    File.WriteAllText(layoutPath, layoutJson);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
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
}
