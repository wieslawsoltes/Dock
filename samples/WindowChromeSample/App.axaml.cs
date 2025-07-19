using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace WindowChromeSample;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl();
            var factory = new Factory();

            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = true,
                EnableWindowDrag = true
            };

            documentDock.DocumentFactory = () =>
            {
                var index = documentDock.VisibleDockables?.Count ?? 0;
                return new Document { Id = $"Doc{index + 1}", Title = $"Document {index + 1}" };
            };

            var document = new Document { Id = "Doc1", Title = "Document 1" };
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(documentDock);
            root.DefaultDockable = documentDock;

            factory.InitLayout(root);
            dockControl.Factory = factory;
            dockControl.Layout = root;

            desktop.MainWindow = new HostWindow
            {
                Width = 800,
                Height = 600,
                Title = "Window Chrome Sample",
                DocumentChromeControlsWholeWindow = true,
                Content = dockControl
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
