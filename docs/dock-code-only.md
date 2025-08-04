# Dock Code-Only Guide

This guide shows how to create a minimal Dock layout entirely in C#. It does not rely on MVVM helpers or XAML markup. The example uses the `Dock.Model.Avalonia` factory together with `DockControl`.

## Step-by-step tutorial

This approach is ideal5. **Run the application**

   ```bash
   dotnet run
   ```

The application will show a simple docking interface with one document tab. This approach gives you complete programmatic control over the layout creation and management without relying on XAML or MVVM patterns.ou need complete programmatic control over the layout without any XAML or when building dynamic layouts at runtime. For document management with data binding, consider the [ItemsSource approach](dock-itemssource.md) instead.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MinimalDockApp
   cd MinimalDockApp
   ```

2. **Install the Dock packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Avalonia
   dotnet add package Dock.Avalonia.Themes.Fluent
   # or use Dock.Avalonia.Themes.Simple
   ```

   **Optional packages for serialization (choose one):**
   ```bash
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   dotnet add package Dock.Serializer.Protobuf          # Binary
   dotnet add package Dock.Serializer.Xml               # XML
   dotnet add package Dock.Serializer.Yaml              # YAML
   ```

3. **Set up View Locator (Required)**

   Even for code-only layouts, you need a view locator for document and tool content:

   **Option A: Simple View Locator for Basic Content**

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;

   namespace MinimalDockApp;

   public class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           return data switch
           {
               IDockable dockable when !string.IsNullOrEmpty(dockable.Title) => 
                   new TextBox { Text = $"Content for {dockable.Title}", AcceptsReturn = true },
               string text => new TextBox { Text = text, AcceptsReturn = true },
               _ => new TextBlock { Text = data?.ToString() ?? "No Content" }
           };
       }

       public bool Match(object? data) => true;
   }
   ```

   **Option B: Static View Locator (if using view models)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using StaticViewLocator;

   namespace MinimalDockApp;

   [StaticViewLocator]
   public partial class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           var type = data.GetType();
           if (s_views.TryGetValue(type, out var func))
               return func.Invoke();

           // Fallback for basic content
           return new TextBox { Text = data.ToString(), AcceptsReturn = true };
       }

       public bool Match(object? data) => true;
   }
   ```

4. **Initialize Dock purely in code**

   This example shows a complete, minimal application that creates a simple layout with one document. The view locator handles basic content display, and the factory manages the dock structure. You can extend this by adding tools, multiple documents, and custom content as needed.

5. **Run the application**
   using Avalonia;
   using Avalonia.Controls;
   using Avalonia.Controls.ApplicationLifetimes;
   using Avalonia.Markup.Xaml;
   using Avalonia.Styling;
   using Avalonia.Themes.Fluent;
   using Dock.Avalonia.Controls;
   using Dock.Avalonia.Themes.Fluent;
   using Dock.Model.Avalonia;
   using Dock.Model.Avalonia.Controls;
   using Dock.Model.Core;

   namespace MinimalDockApp;

   public class App : Application
   {
       public override void Initialize()
       {
           // Load view locator
           DataTemplates.Add(new ViewLocator());
           
           // Load themes
           Styles.Add(new FluentTheme());
           Styles.Add(new DockFluentTheme());
           
           AvaloniaXamlLoader.Load(this);
       }

       public override void OnFrameworkInitializationCompleted()
       {
           var factory = new Factory();

           // Create a simple document
           var document = new Document { Id = "Doc1", Title = "Welcome" };

           // Create a basic layout
           var documentDock = new DocumentDock
           {
               VisibleDockables = factory.CreateList<IDockable>(document),
               ActiveDockable = document
           };

           var layout = new RootDock
           {
               VisibleDockables = factory.CreateList<IDockable>(documentDock),
               ActiveDockable = documentDock,
               DefaultDockable = documentDock
           };

           // Initialize the layout
           factory.InitLayout(layout);

           // Create and show the main window
           var mainWindow = new Window
           {
               Title = "Minimal Dock App",
               Content = new DockControl { Layout = layout }
           };

           if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
           {
               desktop.MainWindow = mainWindow;
           }

           base.OnFrameworkInitializationCompleted();
       }
   }

   internal class Program
   {
       [STAThread]
       private static void Main(string[] args)
       {
           BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
       }

       public static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .WithInterFont()
               .LogToTrace();
   }

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
