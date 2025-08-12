using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;

namespace DockSimplifiedFluentSample;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var factory = new Factory();

            factory.Document(out var document, d => d.WithId("Document1").WithTitle("Document 1"))
                   .Tool(out var tool, t => t.WithId("Tool1").WithTitle("Tool 1"))
                .DocumentDock(out var documentDock, d => d
                    .WithId("Documents")
                    .WithTitle("Documents")
                    .AppendDocument(document)
                    .WithActiveDockable(document)
                    .WithProportion(0.75))
                .ToolDock(out var toolDock, Alignment.Left, d => d
                    .WithId("Tools")
                    .WithTitle("Tools")
                    .AppendTool(tool)
                    .WithActiveDockable(tool)
                    .WithProportion(0.25))
                .ProportionalDockSplitter(out var splitter, canResize: true)
                .ProportionalDock(out var mainLayout, Orientation.Horizontal, d => d
                    .WithId("MainLayout")
                    .Add(toolDock, splitter, documentDock))
                .RootDock(out var rootDock, r => r
                    .WithId("Root")
                    .WithTitle("Root")
                    .Add(mainLayout)
                    .WithActiveDockable(mainLayout));
  
            var mainWindow = new MainWindow
            {
                Content = new DockControl
                {
                    Factory = factory,
                    Layout = rootDock,
                    InitializeFactory = true,
                    InitializeLayout = true
                }
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
