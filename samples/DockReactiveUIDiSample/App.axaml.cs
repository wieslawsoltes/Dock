using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels;
using DockReactiveUIDiSample.Views;
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
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
