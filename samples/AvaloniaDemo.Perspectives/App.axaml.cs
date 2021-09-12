using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;

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

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                mainWindow.Closing += (sender, e) =>
                {
                    mainWindowViewModel.DetachDockControl();

                    // TODO: Save main window position, size and state.
                };

                // TODO: Restore main window position, size and state.

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (sennder, e) =>
                {
                    mainWindowViewModel.DetachDockControl();
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                var mainView = new MainView()
                {
                    DataContext = mainWindowViewModel
                };

                singleViewLifetime.MainView = mainView;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
