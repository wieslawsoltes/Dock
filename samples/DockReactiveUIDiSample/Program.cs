using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Dock.Model;
using Dock.Model.Core;
using Dock.Serializer;
using DockReactiveUIDiSample.Models;
using DockReactiveUIDiSample.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DockReactiveUIDiSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        App.ServiceProvider = provider;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DemoData>();
        services.AddTransient<DocumentViewModel>();
        services.AddTransient<ToolViewModel>();
        services.AddSingleton<DockFactory>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFactory, DockFactory>();
        services.AddSingleton<IDockSerializer, DockSerializer>();
        services.AddSingleton<IDockState, DockState>();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
