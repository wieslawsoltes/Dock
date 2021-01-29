using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Models;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var mainWindowViewModel = new MainWindowViewModel();
            var factory = new DemoFactory(new DemoData());
            IDock? layout = null;

            switch (ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktopLifetime:
                {
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };

                    mainWindowViewModel.Factory = factory;
                    mainWindowViewModel.Layout = layout ?? mainWindowViewModel.Factory?.CreateLayout();

                    if (mainWindowViewModel.Layout != null)
                    {
                        mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                        if (mainWindowViewModel.Layout is IRootDock root)
                        {
                            root.Navigate.Execute("Home");
                        }
                    }

                    mainWindow.Closing += (_, _) =>
                    {
                        if (mainWindowViewModel.Layout is IDock dock)
                        {
                            if (dock.Close.CanExecute(null))
                            {
                                dock.Close.Execute(null);
                            }
                        }
                    };

                    desktopLifetime.MainWindow = mainWindow;

                    desktopLifetime.Exit += (_, _) =>
                    {
                        if (mainWindowViewModel.Layout is IDock dock)
                        {
                            if (dock.Close.CanExecute(null))
                            {
                                dock.Close.Execute(null);
                            }
                        }
                    };
                    break;
                }
                case ISingleViewApplicationLifetime singleViewLifetime:
                {
                    var mainView = new MainView()
                    {
                        DataContext = mainWindowViewModel
                    };

                    mainWindowViewModel.Factory = factory;
                    mainWindowViewModel.Layout = layout ?? mainWindowViewModel.Factory?.CreateLayout();

                    if (mainWindowViewModel.Layout != null)
                    {
                        mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                    }

                    singleViewLifetime.MainView = mainView;
                    break;
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
