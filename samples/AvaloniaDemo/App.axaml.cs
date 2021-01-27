using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Models;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model;
using Dock.Model.Controls;

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

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
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
                }

                mainWindow.Closing += (_, _) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                };

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (_, _) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
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
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
