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
using Dock.Settings;

namespace PinnedWindowSample;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        DockSettings.UsePinnedDockWindow = true;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    static AppBuilder BuildAvaloniaApp() =>
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
            var factory = new Factory();

            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = true
            };

            documentDock.DocumentFactory = () =>
            {
                var index = documentDock.VisibleDockables?.Count ?? 0;
                return new Document { Id = $"Doc{index + 1}", Title = $"Document {index + 1}" };
            };

            var document = new Document { Id = "Doc1", Title = "Document 1" };
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;

            var leftTool = new Tool { Id = "Tool1", Title = "Tool 1" };
            var bottomTool = new Tool { Id = "Tool2", Title = "Output" };

            var leftDock = new ToolDock
            {
                Id = "LeftPane",
                Alignment = Alignment.Left,
                Proportion = 0.25,
                VisibleDockables = factory.CreateList<IDockable>(leftTool),
                ActiveDockable = leftTool
            };

            var bottomDock = new ToolDock
            {
                Id = "BottomPane",
                Alignment = Alignment.Bottom,
                Proportion = 0.25,
                VisibleDockables = factory.CreateList<IDockable>(bottomTool),
                ActiveDockable = bottomTool
            };

            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = factory.CreateList<IDockable>(
                    leftDock,
                    new ProportionalDockSplitter(),
                    documentDock,
                    new ProportionalDockSplitter(),
                    bottomDock)
            };

            var root = factory.CreateRootDock();
            root.LeftPinnedDockables = factory.CreateList<IDockable>();
            root.BottomPinnedDockables = factory.CreateList<IDockable>();
            root.PinnedDock = new ToolDock();
            root.VisibleDockables = factory.CreateList<IDockable>(mainLayout);
            root.DefaultDockable = mainLayout;

            factory.InitLayout(root);
            factory.PinDockable(leftTool);
            factory.PinDockable(bottomTool);

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

