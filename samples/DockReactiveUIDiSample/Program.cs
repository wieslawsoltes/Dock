using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Dock.Model;
using Dock.Model.Core;
using Dock.Serializer;
using DockReactiveUIDiSample.Models;
using DockReactiveUIDiSample.ViewModels;
using DockReactiveUIDiSample.ViewModels.Documents;
using DockReactiveUIDiSample.ViewModels.Tools;
using DockReactiveUIDiSample.Views.Documents;
using DockReactiveUIDiSample.Views.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace DockReactiveUIDiSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using var provider = Initialize();
        BuildAvaloniaApp(provider).StartWithClassicDesktopLifetime(args);
    }

    private static ServiceProvider Initialize()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        return provider;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<App>();
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<DemoData>();
        services.AddTransient<DocumentViewModel>();
        services.AddTransient<ToolViewModel>();
        services.AddTransient<DocumentView>();
        services.AddTransient<ToolView>();
        services.AddSingleton<DockFactory>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFactory, DockFactory>();
        services.AddSingleton<IDockSerializer, DockSerializer>();
        services.AddSingleton<IDockState, DockState>();
   }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider provider)
        => AppBuilder.Configure(provider.GetRequiredService<App>)
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => Initialize().GetRequiredService<App>())
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
