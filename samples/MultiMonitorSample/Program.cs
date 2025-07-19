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

namespace MultiMonitorSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
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
            var factory = new MultiMonitorFactory();

            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false
            };

            var document = new Document { Id = "Doc1", Title = "Document 1" };
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;

            var tool = new Tool { Id = "Tool1", Title = "Tool 1" };

            var layout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = factory.CreateList<IDockable>(
                    documentDock,
                    new ProportionalDockSplitter(),
                    new ToolDock
                    {
                        Id = "Tools",
                        Alignment = Alignment.Right,
                        Proportion = 0.25,
                        VisibleDockables = factory.CreateList<IDockable>(tool),
                        ActiveDockable = tool
                    })
            };

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(layout);
            root.DefaultDockable = layout;

            factory.InitLayout(root);

            // Float the tool into its own window on another monitor
            factory.SplitToWindow(layout, tool, 100, 100, 400, 300);

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

public class MonitorHostWindow : HostWindow
{
    private readonly int _screenIndex;

    public MonitorHostWindow(int screenIndex)
    {
        _screenIndex = screenIndex;
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        var all = Screens.All;
        if (_screenIndex >= 0 && _screenIndex < all.Count)
        {
            Position = all[_screenIndex].WorkingArea.Position;
        }
    }
}

public class MultiMonitorFactory : Factory
{
    private int _nextMonitor;

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            ["Monitor1"] = () => new MonitorHostWindow(0),
            ["Monitor2"] = () => new MonitorHostWindow(1)
        };

        DefaultHostWindowLocator = () => new MonitorHostWindow(0);

        base.InitLayout(layout);
    }

    public override IDockWindow CreateDockWindow()
    {
        var window = base.CreateDockWindow();
        window.Id = _nextMonitor == 0 ? "Monitor1" : "Monitor2";
        _nextMonitor = (_nextMonitor + 1) % 2;
        return window;
    }
}
