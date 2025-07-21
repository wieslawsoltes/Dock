using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Dock.Model.Extensions.DependencyInjection;
using Dock.Serializer;
using DockReactiveUIDiSample.Models;
using DockReactiveUIDiSample.ViewModels;
using DockReactiveUIDiSample.ViewModels.Documents;
using DockReactiveUIDiSample.ViewModels.Tools;
using DockReactiveUIDiSample.Views;
using DockReactiveUIDiSample.Views.Documents;
using DockReactiveUIDiSample.Views.Tools;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DockReactiveUIDiSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using var provider = Initialize();
        using var _ = provider.UseDockEventLogger();
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
        services.AddSingleton<IViewLocator, ViewLocator>();
        services.AddSingleton<DemoData>();
        services.AddTransient<DocumentViewModel>();
        services.AddTransient<ToolViewModel>();
        services.AddTransient<IViewFor<DocumentViewModel>, DocumentView>();
        services.AddTransient<IViewFor<ToolViewModel>, ToolView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<IViewFor<MainWindowViewModel>, MainWindow>();

        services.AddDock<DockFactory, DockSerializer>();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.UseDockEventLogger();
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
