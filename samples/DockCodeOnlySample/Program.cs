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

            var factory = new Factory();

            var documentDock = (DocumentDock)factory.CreateDocumentDock("Documents",
                factory.CreateDocument("Doc1", "Document 1"));
            documentDock.IsCollapsable = false;
            documentDock.CanCreateDocument = true;

            documentDock.DocumentFactory = () =>
            {
                var index = documentDock.VisibleDockables?.Count ?? 0;
                return factory.CreateDocument($"Doc{index + 1}", $"Document {index + 1}");
            };

            var leftToolDock = factory.CreateToolDock("LeftPane", Alignment.Left,
                factory.CreateTool("Tool1", "Tool 1"));
            leftToolDock.Proportion = 0.25;

            var bottomToolDock = factory.CreateToolDock("BottomPane", Alignment.Bottom,
                factory.CreateTool("Tool2", "Output"));
            bottomToolDock.Proportion = 0.25;

            var mainLayout = factory.CreateProportionalDock(Orientation.Horizontal,
                leftToolDock,
                factory.CreateProportionalDockSplitter(),
                documentDock,
                factory.CreateProportionalDockSplitter(),
                bottomToolDock);

            var root = factory.CreateRootDock(mainLayout);

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
