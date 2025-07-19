# Quick Start

This short guide shows how to set up Dock in a new Avalonia application. You will install the NuGet packages, create a minimal layout and run it.

## Step-by-step tutorial

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o DockQuickStart
   cd DockQuickStart
   ```

2. **Install the Dock packages**

   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Mvvm
   ```

3. **Add Dock styles**

   Reference one of the built-in themes in `App.axaml` so the controls are styled:

   ```xaml
   <Application.Styles>
     <FluentTheme Mode="Dark" />
     <DockFluentTheme />
   </Application.Styles>
   ```

4. **Add a simple layout**

   Create a `DockFactory` that derives from `Dock.Model.Mvvm.Factory` and returns a basic layout:

   ```csharp
   using Dock.Model.Core;
   using Dock.Model.Mvvm;
   using Dock.Model.Mvvm.Controls;

   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var document = new Document { Id = "Doc1", Title = "Document" };

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = CreateList<IDockable>(document),
                   ActiveDockable = document
               });
           root.DefaultDockable = root.VisibleDockables[0];
           return root;
       }
   }
  ```

   Alternatively you can build the same layout using `DockLayoutBuilder`:

   ```csharp
   var layout = new DockLayoutBuilder(new DockFactory())
       .WithDocument(new Document { Id = "Doc1", Title = "Document" })
       .Build();
   ```

   Initialize this layout in `MainWindow.axaml.cs`:

   ```csharp
   public partial class MainWindow : Window
   {
       private readonly DockFactory _factory = new();

       public MainWindow()
       {
           InitializeComponent();
           var layout = _factory.CreateLayout();
           _factory.InitLayout(layout);
           Dock.Layout = layout;
       }
   }
   ```

   Add a `DockControl` placeholder to `MainWindow.axaml`:

   ```xaml
   <DockControl x:Name="Dock" />
   ```

5. **Run the application**

   ```bash
   dotnet run
   ```

The window should show a single document hosted by `DockControl`. See the other guides for more advanced layouts.
