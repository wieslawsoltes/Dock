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
        // Resolve application using dependency injection
        var _ = provider.GetRequiredService<App>();
        BuildAvaloniaApp(provider).StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<App>();
        services.AddSingleton<DemoData>();
        services.AddTransient<DocumentViewModel>();
        services.AddTransient<ToolViewModel>();
        services.AddTransient<Views.Documents.DocumentView>();
        services.AddTransient<Views.Tools.ToolView>();
        services.AddTransient<Views.DockableOptionsView>();
        services.AddSingleton<DockFactory>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFactory, DockFactory>();
        services.AddSingleton<IDockSerializer, DockSerializer>();
        services.AddSingleton<IDockState, DockState>();
   }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider provider)
        => AppBuilder.Configure(() => provider.GetRequiredService<App>())
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
