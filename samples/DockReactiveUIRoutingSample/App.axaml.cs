using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using DockReactiveUIRoutingSample.ViewModels;
using DockReactiveUIRoutingSample.Views;

namespace DockReactiveUIRoutingSample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var vm = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
#if DEBUG
            desktop.MainWindow.AttachDockDebug(
                vm.Layout, 
                new KeyGesture(Key.F11));
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}
