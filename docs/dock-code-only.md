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
   dotnet add package Dock.Serializer
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
   using Dock.Avalonia.Themes;
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

              // Build a layout using the factory convenience methods
              var factory  = new Factory();

              var documentDock = factory.CreateDocumentDock("Documents",
                  factory.CreateDocument("Doc1", "Document"));

              var leftToolDock = factory.CreateToolDock("LeftPane", Alignment.Left,
                  factory.CreateTool("Tool1", "Tool 1"));
              leftToolDock.Proportion = 0.25;

              var bottomToolDock = factory.CreateToolDock("BottomPane", Alignment.Bottom,
                  factory.CreateTool("Tool2", "Output"));
              bottomToolDock.Proportion = 0.25;

              var mainLayout = factory.CreateProportionalDock(Orientation.Horizontal,
                  leftToolDock,
                  factory.CreateProportionalDockSplitter(),
                  documentDock,
                  factory.CreateProportionalDockSplitter(),
                  bottomToolDock);

              var root = factory.CreateRootDock(mainLayout);
              
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

You can find a complete project in the repository under
[`samples/DockCodeOnlySample`](../samples/DockCodeOnlySample).
