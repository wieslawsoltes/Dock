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
using Dock.Serializer;
using ReactiveUI.Legacy;

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

#pragma warning disable CS0618 // Type or member is obsolete
            mainWindowViewModel.Serializer = new DockSerializer(typeof(ReactiveList<>));
#pragma warning restore CS0618 // Type or member is obsolete

            var factory = new DemoFactory(new DemoData());
            IDock layout = null;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
                if (File.Exists(path))
                {
                    layout = mainWindowViewModel.Serializer.Load<RootDock>(path);
                }

                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                // TODO: Restore main window position, size and state.

                mainWindowViewModel.Factory = factory;
                mainWindowViewModel.Layout = layout ?? mainWindowViewModel.Factory?.CreateLayout();
                mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);

                mainWindow.Closing += (sender, e) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    // TODO: Save main window position, size and state.
                };

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (sennder, e) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    mainWindowViewModel.Serializer.Save(path, mainWindowViewModel.Layout);
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
                mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);

                singleViewLifetime.MainView = mainView;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
