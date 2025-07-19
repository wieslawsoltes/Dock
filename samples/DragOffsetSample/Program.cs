using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Contract;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace DragOffsetSample;

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
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl
            {
                DragOffsetCalculator = new CenteredDragOffsetCalculator()
            };

            var factory = new Factory();
            var document = new Document { Id = "Doc1", Title = "Document" };
            var documentDock = new DocumentDock
            {
                Id = "Documents",
                VisibleDockables = factory.CreateList<IDockable>(document),
                ActiveDockable = document
            };
            var root = new RootDock
            {
                VisibleDockables = factory.CreateList<IDockable>(documentDock),
                DefaultDockable = documentDock
            };

            factory.InitLayout(root);
            dockControl.Layout = root;
            dockControl.Factory = factory;

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

public class CenteredDragOffsetCalculator : IDragOffsetCalculator
{
    public PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition)
    {
        var bounds = dragControl.Bounds;
        return new PixelPoint(-(int)(bounds.Width / 2), -(int)(bounds.Height / 2));
    }
}
