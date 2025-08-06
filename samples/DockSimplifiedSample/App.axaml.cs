using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockSimplifiedSample;

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
            var factory = new Factory(CreateLayout);
            var layout = factory.CreateLayout();

            var mainWindow = new MainWindow
            {
                Content = new DockControl
                {
                    Factory = factory,
                    Layout = layout,
                    InitializeFactory = true,
                    InitializeLayout = true
                }
            };
            
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private static IRootDock CreateLayout()
    {
        var document = new Document { Id = "Document1", Title = "Document 1" };
        var tool = new Tool { Id = "Tool1", Title = "Tool 1" };
        
        var documentDock = new DocumentDock
        {
            Id = "Documents",
            Title = "Documents",
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document
        };
        
        var toolDock = new ToolDock
        {
            Id = "Tools",
            Title = "Tools",
            VisibleDockables = new List<IDockable> { tool },
            ActiveDockable = tool,
            Alignment = Alignment.Left
        };
        
        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> 
            { 
                toolDock, 
                new ProportionalDockSplitter { CanResize = true }, 
                documentDock 
            }
        };
        
        toolDock.Proportion = 0.25;
        documentDock.Proportion = 0.75;
        
        var rootDock = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = new List<IDockable> { mainLayout },
            ActiveDockable = mainLayout
        };
        
        return rootDock;
    }
}
