using System;
using System.IO;
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
using Dock.Model.Serialization;

namespace MultiWindowSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
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
            var factory = new MultiFactory();
            var serializer = new DockSerializer();
            var state = new DockState();

            const string path = "layout.json";
            IRootDock layout;
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                layout = serializer.Load<IRootDock?>(stream) ?? factory.CreateLayout();
                factory.InitLayout(layout);
                state.Restore(layout);
            }
            else
            {
                layout = factory.CreateLayout();
                factory.InitLayout(layout);

                var floating = factory.CreateLayout();
                factory.InitLayout(floating);
                factory.FloatDockable(layout, floating);
            }

            dockControl.Factory = factory;
            dockControl.Layout = layout;

            desktop.MainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl
            };

            desktop.Exit += (_, _) =>
            {
                state.Save(layout);
                using var stream = File.Create(path);
                serializer.Save(stream, layout);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public class MultiFactory : Factory
{
    private int _index;

    public override IRootDock CreateLayout()
    {
        var count = ++_index;
        var doc = new Document { Id = $"Doc{count}", Title = $"Document {count}" };

        var root = CreateRootDock();
        root.VisibleDockables = CreateList<IDockable>(
            new DocumentDock
            {
                VisibleDockables = CreateList<IDockable>(doc),
                ActiveDockable = doc
            });
        return root;
    }

    public override IHostWindow CreateWindowFrom(IDockWindow source)
    {
        var window = base.CreateWindowFrom(source);
        window.Title = $"Dock - {source.Id}";
        return window;
    }
}
