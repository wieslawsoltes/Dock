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
    public IServiceProvider? ServiceProvider { get; set; }

    public App(IServiceProvider? serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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
