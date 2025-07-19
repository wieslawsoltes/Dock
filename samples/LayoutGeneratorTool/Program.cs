using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;
using Dock.Model;

namespace LayoutGeneratorTool;

internal class Program
{
    private static string _config = "layout.json";

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            _config = args[0];
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(_config))
            .UsePlatformDetect()
            .LogToTrace();
}

internal class App : Application
{
    private readonly string _file;

    public App(string file)
    {
        _file = file;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var layout = LayoutGenerator.Load(_file);
            var factory = new Factory();
            layout ??= factory.CreateLayout();

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            desktop.MainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl,
                Title = "Layout Generator"
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
