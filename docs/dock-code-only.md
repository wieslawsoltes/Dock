# Dock Code-Only Guide

This guide shows how to build a minimal Dock layout entirely in C# using
`Dock.Model.Avalonia` and `DockControl`. It mirrors the approach used in
`samples/DockCodeOnlySample` and avoids XAML or MVVM helpers.

## Create a new Avalonia project

```bash
dotnet new avalonia.app -o MinimalDockApp
cd MinimalDockApp
```

## Install the Dock packages

```bash
dotnet add package Dock.Avalonia
dotnet add package Dock.Model.Avalonia
dotnet add package Dock.Avalonia.Themes.Fluent
# or use Dock.Avalonia.Themes.Simple
```

Optional serializers (choose one):

```bash
dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
dotnet add package Dock.Serializer.Protobuf          # Binary
dotnet add package Dock.Serializer.Xml               # XML
dotnet add package Dock.Serializer.Yaml              # YAML
```

## Minimal code-only app

Replace your `Program.cs` with the following:

```csharp
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace MinimalDockApp;

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
        RequestedThemeVariant = ThemeVariant.Light;

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
                return new Document
                {
                    Id = $"Doc{index + 1}",
                    Title = $"Document {index + 1}",
                    Content = new TextBox { Text = $"Document {index + 1}", AcceptsReturn = true }
                };
            };

            var document = new Document
            {
                Id = "Doc1",
                Title = "Document 1",
                Content = new TextBox { Text = "Document 1", AcceptsReturn = true }
            };

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
```

## Run the application

```bash
dotnet run
```

The window will show a document dock flanked by left and bottom tool panes.
`DocumentDock` exposes `AddDocument` for adding and activating documents in
code, and `ToolDock` provides `AddTool` for tool panes.

## Notes on content

`Document.Content` can be set to an Avalonia control instance (as shown above),
which makes it easy to host simple UI without a view locator or data templates.
For larger applications, consider using data templates or the
[ItemsSource approach](dock-itemssource.md) instead.

You can find a complete project in the repository under
`samples/DockCodeOnlySample`.
