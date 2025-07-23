# Dock Code-Only Guide

This guide shows how to create a minimal Dock layout entirely in C#. It does not rely on MVVM helpers or XAML markup. The example uses the `Dock.Model.Avalonia` factory together with `DockControl`.

## Step-by-step tutorial

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MinimalDockApp
   cd MinimalDockApp
   ```

2. **Install the Dock packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Avalonia
   dotnet add package Dock.Serializer.Newtonsoft
   # or use the Protobuf variant
   dotnet add package Dock.Serializer.Protobuf
   ```

3. **Initialize Dock purely in code**

   The program below builds a simple layout with one document and two tool docks and assigns it to `DockControl`:

   ```csharp
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

               // Create a layout using the plain Avalonia factory
               var factory  = new Factory();
               var document = new Document { Id = "Doc1", Title = "Document" };
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
                       new DocumentDock
                       {
                           Id = "Documents",
                           VisibleDockables = factory.CreateList<IDockable>(document),
                           ActiveDockable = document
                       },
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

4. **Run the application**

   ```bash
   dotnet run
   ```

The window will show a document dock flanked by left and bottom tool panes without using XAML or MVVM helpers.
`DocumentDock` exposes an `AddDocument` method for adding and activating documents in code, while `ToolDock` provides `AddTool` for tool panes.

### Attaching a `UserControl`

Documents can host any Avalonia control directly. Assign a control instance to
`Content` before adding the document:

```csharp
var document = new Document
{
    Title = "View",
    Content = new MyUserControl()
};
documentDock.AddDocument(document);
```

You can find a complete project in the repository under
[`samples/DockCodeOnlySample`](../samples/DockCodeOnlySample).
