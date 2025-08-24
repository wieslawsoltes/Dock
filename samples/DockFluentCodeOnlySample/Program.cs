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

namespace DockFluentCodeOnlySample;

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
            var factory = new Factory();

            // Create tools
            factory.Tool(out var explorerTool, t => t.WithId("Explorer").WithTitle("Explorer"))
                   .Tool(out var searchTool, t => t.WithId("Search").WithTitle("Search"))
                   .Tool(out var propertiesTool, t => t.WithId("Properties").WithTitle("Properties"))
                   .Tool(out var outlineTool, t => t.WithId("Outline").WithTitle("Outline"))
                   .Tool(out var outputTool, t => t.WithId("Output").WithTitle("Output"))
                   .Tool(out var errorsTool, t => t.WithId("Errors").WithTitle("Errors"))
                   .Tool(out var terminalTool, t => t.WithId("Terminal").WithTitle("Terminal"))

                // Create documents
                .Document(out var docHome, d => d.WithId("DocHome").WithTitle("Home"))
                .Document(out var docReadme, d => d.WithId("DocReadme").WithTitle("Readme.md"))
                .Document(out var docSettings, d => d.WithId("DocSettings").WithTitle("Settings.json"))

                // First document dock (left side in center)
                .DocumentDock(out var docsLeft, d => d
                    .WithId("DocsLeft")
                    .WithTitle("Documents (Left)")
                    .WithIsCollapsable(false)
                    .WithEnableWindowDrag(true)
                    .WithTabsLayout(DocumentTabLayout.Top)
                    .WithCanCreateDocument(true)
                    .AppendDocument(docHome)
                    .AppendDocument(docReadme)
                    .WithActiveDockable(docHome))

                // Second document dock (right side in center)
                .DocumentDock(out var docsRight, d => d
                    .WithId("DocsRight")
                    .WithTitle("Documents (Right)")
                    .WithIsCollapsable(false)
                    .WithEnableWindowDrag(true)
                    .WithTabsLayout(DocumentTabLayout.Right)
                    .WithCanCreateDocument(true)
                    .AppendDocument(docSettings)
                    .WithActiveDockable(docSettings))

                // Left / Right tool panes
                .ToolDock(out var leftPane, Alignment.Left, d => d
                    .WithId("LeftPane")
                    .WithTitle("Left Pane")
                    .AppendTool(explorerTool)
                    .AppendTool(searchTool)
                    .WithActiveDockable(explorerTool)
                    .WithProportion(0.2))
                .ToolDock(out var rightPane, Alignment.Right, d => d
                    .WithId("RightPane")
                    .WithTitle("Right Pane")
                    .AppendTool(propertiesTool)
                    .AppendTool(outlineTool)
                    .WithActiveDockable(propertiesTool)
                    .WithProportion(0.2))

                // Bottom tool pane
                .ToolDock(out var bottomPane, Alignment.Bottom, d => d
                    .WithId("BottomPane")
                    .WithTitle("Bottom Pane")
                    .AppendTool(outputTool)
                    .AppendTool(errorsTool)
                    .AppendTool(terminalTool)
                    .WithActiveDockable(outputTool)
                    .WithProportion(0.25))

                // Splitters used in the layout
                .ProportionalDockSplitter(out var splitH1)
                .ProportionalDockSplitter(out var splitH2)
                .ProportionalDockSplitter(out var splitV)

                // Center area: horizontally split into two document docks
                .ProportionalDock(out var centerHorizontal, Orientation.Horizontal, d => d
                    .WithId("CenterHorizontal")
                    .WithTitle("Center")
                    .Add(docsLeft, splitH1, docsRight))

                // Center container: vertically split docs over the bottom tool pane
                .ProportionalDock(out var centerVertical, Orientation.Vertical, d => d
                    .WithId("CenterVertical")
                    .Add(centerHorizontal, splitV, bottomPane))

                // Main layout: left pane | center | right pane
                .ProportionalDock(out var mainLayout, Orientation.Horizontal, d => d
                    .WithId("MainLayout")
                    .Add(leftPane, splitH2, centerVertical, rightPane))

                // Root dock to host everything
                .RootDock(out var root, r => r
                    .WithId("Root")
                    .WithTitle("Root")
                    .Add(mainLayout)
                    .WithDefaultDockable(mainLayout)
                    .WithActiveDockable(mainLayout)
                    .WithEnableGlobalDocking(true));

            // Optional: pinned tools on the root
            root.WithLeftPinned(factory.Tool(t => t.WithId("PinnedLeft").WithTitle("Pinned Left")))
                .WithBottomPinned(factory.Tool(t => t.WithId("PinnedBottom").WithTitle("Pinned Bottom")));

            // Wire document factory for dynamic document creation
            if (docsLeft is DocumentDock docsLeftImpl)
            {
                docsLeftImpl.DocumentFactory = () =>
                {
                    var index = docsLeft.VisibleDockables?.Count ?? 0;
                    return new Document
                    {
                        Id = $"DocL{index + 1}",
                        Title = $"Left Doc {index + 1}"
                    };
                };
            }

            if (docsRight is DocumentDock docsRightImpl)
            {
                docsRightImpl.DocumentFactory = () =>
                {
                    var index = docsRight.VisibleDockables?.Count ?? 0;
                    return new Document
                    {
                        Id = $"DocR{index + 1}",
                        Title = $"Right Doc {index + 1}"
                    };
                };
            }

            // Initialize and host in DockControl
            factory.InitLayout(root);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = root,
                InitializeFactory = true,
                InitializeLayout = true
            };

            desktop.MainWindow = new Window
            {
                Width = 1200,
                Height = 800,
                Title = "Dock Fluent Code-Only Sample",
                Content = dockControl
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
