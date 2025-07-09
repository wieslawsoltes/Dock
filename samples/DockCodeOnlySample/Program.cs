using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes;
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
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl();

            // Create a layout using the plain Avalonia factory
            var factory  = new Factory();

            var document = new Document { Id = "Doc1", Title = "Document" };
            var leftTool = new Tool { Id = "Tool1", Title = "Tool 1" };
            var bottomTool = new Tool { Id = "Tool2", Title = "Output" };

            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = factory.CreateList<IDockable>(
                    new ToolDock
                    {
                        Id = "LeftPane",
                        Alignment = Alignment.Left,
                        Proportion = 0.25,
                        VisibleDockables = factory.CreateList<IDockable>(leftTool),
                        ActiveDockable = leftTool
                    },
                    new ProportionalDockSplitter(),
                    new DocumentDock
                    {
                        Id = "Documents",
                        VisibleDockables = factory.CreateList<IDockable>(document),
                        ActiveDockable = document
                    },
                    new ProportionalDockSplitter(),
                    new ToolDock
                    {
                        Id = "BottomPane",
                        Alignment = Alignment.Bottom,
                        Proportion = 0.25,
                        VisibleDockables = factory.CreateList<IDockable>(bottomTool),
                        ActiveDockable = bottomTool
                    })
            };

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(mainLayout);
            root.DefaultDockable = mainLayout;

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
