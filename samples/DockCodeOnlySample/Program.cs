using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace DockCodeOnlySample;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl();

            // Create a layout using the plain Avalonia factory
            var factory  = new Factory();
            var document = new Document { Id = "Doc1", Title = "Document" };

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(
                new DocumentDock
                {
                    VisibleDockables = factory.CreateList<IDockable>(document),
                    ActiveDockable = document
                });

            root.DefaultDockable = root.VisibleDockables[0];

            factory.InitLayout(root);
            dockControl.Factory = factory;
            dockControl.Layout  = root;

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
