using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace BrowserTabTheme;

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

            factory.Document(out var homeDoc, d => d.WithId("Home").WithTitle("Welcome.md"))
                   .Document(out var readmeDoc, d => d.WithId("Readme").WithTitle("README.md"))
                   .Document(out var settingsDoc, d => d.WithId("Settings").WithTitle("settings.json"))
                   .Document(out var changelogDoc, d => d.WithId("Changelog").WithTitle("CHANGELOG.md"))
                   .DocumentDock(out var documentDock, d => d
                       .WithId("Documents")
                       .WithTitle("Documents")
                       .WithIsCollapsable(false)
                       .WithCanCreateDocument(true)
                       .WithEnableWindowDrag(true)
                       .WithTabsLayout(DocumentTabLayout.Top)
                       .AppendDocument(homeDoc)
                       .AppendDocument(readmeDoc)
                       .AppendDocument(settingsDoc)
                       .AppendDocument(changelogDoc)
                       .WithActiveDockable(homeDoc)
                       .WithProportion(1.0))
                   .RootDock(out var rootDock, r => r
                       .WithId("Root")
                       .WithTitle("Root")
                       .Add(documentDock)
                       .WithDefaultDockable(documentDock)
                       .WithActiveDockable(documentDock)
                       .WithEnableGlobalDocking(true));

            if (documentDock is DocumentDock documentDockImpl)
            {
                var nextDocumentIndex = (documentDock.VisibleDockables?.Count ?? 0) + 1;
                documentDockImpl.DocumentFactory = () =>
                {
                    var index = nextDocumentIndex++;
                    return new Document
                    {
                        Id = $"Document{index}",
                        Title = $"New Tab {index}"
                    };
                };
            }

            factory.InitLayout(rootDock);

            var mainWindow = new MainWindow();
            mainWindow.DockControl.Factory = factory;
            mainWindow.DockControl.Layout = rootDock;
            mainWindow.DockControl.InitializeFactory = true;
            mainWindow.DockControl.InitializeLayout = false;

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();

#if DEBUG
        this.AttachDevTools();
#endif
    }
}
