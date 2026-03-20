using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace DockCodeOnlyMvvmSample;

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
            var factory = new Factory();

            // Prepare tools and document
            // Build with one fluent chain
            factory.Tool(out var leftTool, t => t.WithId("Tool1").WithTitle("Tool 1"))
                   .Tool(out var bottomTool, t => t.WithId("Tool2").WithTitle("Output"))
                   .Document(out var doc1, d => d.WithId("Doc1").WithTitle("Document 1"))
                .DocumentDock(out var documentDock, d => d
                    .WithId("Documents")
                    .WithIsCollapsable(false)
                    .WithCanCreateDocument(true))
                .ToolDock(out var leftPane, Alignment.Left, t => t
                    .WithId("LeftPane")
                    .AppendTool(leftTool)
                    .WithActiveDockable(leftTool))
                .ProportionalDockSplitter(out var split1)
                .ProportionalDockSplitter(out var split2)
                .ToolDock(out var bottomPane, Alignment.Bottom, t => t
                    .WithId("BottomPane")
                    .AppendTool(bottomTool)
                    .WithActiveDockable(bottomTool))
                .ProportionalDock(out var mainLayout, Orientation.Horizontal, d => d
                    .Add(leftPane, split1, documentDock, split2, bottomPane))
                .RootDock(out var root, r => r
                    .Add(mainLayout)
                    .WithDefaultDockable(mainLayout)
                    .WithActiveDockable(mainLayout));

            // Proportions
            leftPane.WithProportion(0.25);
            bottomPane.WithProportion(0.25);

            // Document factory and initial document
            if (documentDock is DocumentDock documentDockImpl)
            {
                documentDockImpl.DocumentFactory = () =>
                {
                    var index = documentDock.VisibleDockables?.Count ?? 0;
                    return new Document
                    {
                        Id = $"Doc{index + 1}",
                        Title = $"Document {index + 1}"
                    };
                };
            }

            documentDock.AddDocument(doc1);

            // Initialize and host in DockControl
            factory.InitLayout(root);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = root,
            };

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
