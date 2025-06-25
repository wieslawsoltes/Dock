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

   The program below builds a single document layout and assigns it to `DockControl`:

   ```csharp
   using Avalonia;
   using Avalonia.Controls;
   using Avalonia.Controls.ApplicationLifetimes;
   using Dock.Avalonia.Controls;
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
           if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
           {
               var dockControl = new DockControl();

               // Create a layout using the plain Avalonia factory
               var factory  = new Factory();
               var document = new Document { Id = "Doc1", Title = "Document" };

               var root = factory.CreateRootDock();
               root.VisibleDockables = factory.CreateList<IDockable>(
                   new DocumentDock
                   {
                       VisibleDockables = factory.CreateList<IDockable>(document),
                       ActiveDockable = document
                   });

               root.DefaultDockable = root.VisibleDockables[0];

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

The window will show a single document hosted by `DockControl` without using XAML or MVVM helpers.
