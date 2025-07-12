using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels;
using DockReactiveUIDiSample.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DockReactiveUIDiSample;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; }
    private readonly ViewLocator _viewLocator;

    public App(IServiceProvider? serviceProvider, ViewLocator viewLocator)
    {
        ServiceProvider = serviceProvider;
        _viewLocator = viewLocator;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataTemplates.Insert(0, _viewLocator);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && ServiceProvider != null)
        {
            var vm = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var window = new MainWindow { DataContext = vm };
            window.Closing += async (_, _) => await vm.SaveLayoutAsync();
            desktop.MainWindow = window;
            desktop.Exit += async (_, _) => await vm.SaveLayoutAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
