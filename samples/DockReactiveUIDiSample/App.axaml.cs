using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Diagnostics;
using Dock.Avalonia.Diagnostics;
using DockReactiveUIDiSample.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace DockReactiveUIDiSample;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; }
    private readonly IViewLocator _viewLocator;

    public App(IServiceProvider? serviceProvider, IViewLocator viewLocator)
    {
        ServiceProvider = serviceProvider;
        _viewLocator = viewLocator;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataTemplates.Insert(0, (IDataTemplate)_viewLocator);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && ServiceProvider != null)
        {
            var vm = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var view = ServiceProvider.GetRequiredService<IViewFor<MainWindowViewModel>>();
            view.ViewModel = vm;
            if (view is Window window)
            {
                window.Closing += async (_, _) => await vm.SaveLayoutAsync();
                desktop.MainWindow = window;
                desktop.Exit += async (_, _) => await vm.SaveLayoutAsync();
#if DEBUG
                window.AttachDockDebug(
                    vm.Layout, 
                    new KeyGesture(Key.F11));
                window.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
