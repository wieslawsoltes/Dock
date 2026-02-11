using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Avalonia;
using DockXamlReactiveUISample.ViewModels;
using DockXamlReactiveUISample.Views;

namespace DockXamlReactiveUISample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires dynamic code for MainWindowViewModel.")]
public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(new Factory
                {
                    HideToolsOnClose = false,
                    HideDocumentsOnClose = false
                })
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
