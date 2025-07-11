using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Dock.Settings;
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
        services.AddSingleton<IDockSerializer, Dock.Serializer.DockSerializer>();
        services.AddSingleton<IDockState, Dock.Model.DockState>();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
