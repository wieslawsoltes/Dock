using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace DockBrowserSample;

internal partial class Program
{
    [SupportedOSPlatform("browser")]
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .LogToTrace()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            var dockControl = new DockControl();

            var factory = new Factory();
            factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = () => new OverlayHostWindow()
            };

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
                    documentDock,
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

            single.MainView = dockControl;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
