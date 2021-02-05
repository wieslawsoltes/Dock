using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Controls;
using Notepad.ViewModels;
using Notepad.Views;

namespace Notepad
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
            var factory = new NotepadFactory();

            switch (ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktopLifetime:
                {
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };

                    // TODO: Restore main window position, size and state.

                    mainWindowViewModel.Factory = factory;
                    mainWindowViewModel.Layout = mainWindowViewModel.Factory?.CreateLayout() as IRootDock;
                    if (mainWindowViewModel.Layout is { })
                    {
                        mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                        mainWindowViewModel.Files = mainWindowViewModel.Factory?.FindDockable(mainWindowViewModel.Layout, (d) => d.Id == "Files") as IDocumentDock;
                    }

                    mainWindow.Closing += (_, _) =>
                    {
                        if (mainWindowViewModel.Layout is { } dock)
                        {
                            if (dock.Close.CanExecute(null))
                            {
                                dock.Close.Execute(null);
                            }
                        }
                        // TODO: Save main window position, size and state.
                    };

                    desktopLifetime.MainWindow = mainWindow;

                    desktopLifetime.Exit += (_, _) =>
                    {
                        if (mainWindowViewModel.Layout is { } dock)
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
                    mainWindowViewModel.Layout = mainWindowViewModel.Factory?.CreateLayout() as IRootDock;
                    if (mainWindowViewModel.Layout is { })
                    {
                        mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                        mainWindowViewModel.Files = mainWindowViewModel.Factory?.FindDockable(mainWindowViewModel.Layout, (d) => d.Id == "Files") as IDocumentDock;
                    }

                    singleViewLifetime.MainView = mainView;
                    break;
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
