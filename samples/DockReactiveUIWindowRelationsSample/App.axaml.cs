using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockReactiveUIWindowRelationsSample.ViewModels;
using DockReactiveUIWindowRelationsSample.Views;

namespace DockReactiveUIWindowRelationsSample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires unreferenced code for MainWindowViewModel.")]
public partial class App : Application
{
    public override void Initialize()
    {
#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif
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

                mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();
                desktopLifetime.MainWindow = mainWindow;
                desktopLifetime.Exit += (_, _) => mainWindowViewModel.CloseLayout();
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
    }
}
