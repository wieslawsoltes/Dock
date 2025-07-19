using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using PluginContracts;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginHostSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl();
            var factory = new Factory();

            var documents = new DocumentDock
            {
                Id = "Documents",
                Title = "Documents"
            };

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(documents);
            root.DefaultDockable = documents;

            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
            if (Directory.Exists(pluginsDir))
            {
                foreach (var path in Directory.GetFiles(pluginsDir, "*.dll"))
                {
                    var assembly = Assembly.LoadFrom(path);
                    var plugins = assembly
                        .GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                        .Select(Activator.CreateInstance)
                        .OfType<IPlugin>();

                    foreach (var plugin in plugins)
                    {
                        var dockable = plugin.CreateDockable();
                        factory.AddDockable(documents, dockable);
                    }
                }
            }

            factory.InitLayout(root);
            dockControl.Factory = factory;
            dockControl.Layout = root;

            desktop.MainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

