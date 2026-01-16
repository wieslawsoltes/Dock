# Dock INPC Getting Started Guide

This guide explains how to get started with the INPC (INotifyPropertyChanged) implementation of Dock. The INPC model provides basic property change notifications with lightweight `ICommand` support (via `SimpleCommand`) without requiring a full MVVM framework, making it ideal for simpler scenarios or custom MVVM stacks.

The `Dock.Model.Inpc` package provides `INotifyPropertyChanged` implementations along with simple command helpers used by the built-in dock models. The sample project `DockInpcSample` in the repository demonstrates this approach. For interface details refer to the [Dock API Reference](dock-reference.md).

> **💡 Modern Approach**: For easier document management, consider using [DocumentDock.ItemsSource](dock-itemssource.md) which automatically creates and manages documents from collections. This approach is covered in detail in the [Document and Tool Content Guide](dock-content-guide.md).

## Step-by-step tutorial

Follow these instructions to create a minimal INPC-based application using Dock.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the Dock packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Inpc
   dotnet add package Dock.Avalonia.Themes.Fluent
   ```

   **Optional packages:**
   ```bash
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   
   # For dependency injection:
   dotnet add package Dock.Model.Extensions.DependencyInjection
   ```

3. **Set up View Locator (Required)**

   INPC requires a view locator to map view models to their corresponding views. Choose one of the following approaches:

   **Option A: Static View Locator with Source Generators (Recommended)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using System.ComponentModel;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using StaticViewLocator;

   namespace MyDockApp;

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

           throw new Exception($"Unable to create view for type: {type}");
       }

       public bool Match(object? data)
       {
           if (data is null)
           {
               return false;
           }

           var type = data.GetType();
           return data is IDockable || s_views.ContainsKey(type);
       }
   }
   ```

   **Option B: Convention-Based View Locator**

   ```csharp
   using System;
   using System.ComponentModel;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;

   namespace MyDockApp;

   public class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           var name = data.GetType().FullName!.Replace("ViewModel", "View");
           var type = Type.GetType(name);

           if (type != null)
               return (Control)Activator.CreateInstance(type)!;

           return new TextBlock { Text = "Not Found: " + name };
       }

       public bool Match(object? data)
       {
           if (data is null)
           {
               return false;
           }

           if (data is IDockable)
           {
               return true;
           }

           var name = data.GetType().FullName?.Replace("ViewModel", "View");
           return Type.GetType(name) is not null;
       }
   }
   ```

   Register the view locator in `App.axaml`:
   ```xaml
   <Application xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:MyDockApp"
                x:Class="MyDockApp.App">

     <Application.DataTemplates>
       <local:ViewLocator />
     </Application.DataTemplates>

     <Application.Styles>
       <FluentTheme />
       <DockFluentTheme />
     </Application.Styles>
   </Application>
   ```

4. **Create a factory and view models**

   Derive from `Dock.Model.Inpc.Factory` and implement `CreateLayout`. Your documents and tools should inherit from the INPC versions:

   ```csharp
   using Dock.Model.Core;
   using Dock.Model.Inpc;
   using Dock.Model.Inpc.Controls;

   namespace MyDockApp.ViewModels;

   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var doc = new DocumentViewModel { Id = "Doc1", Title = "Document" };
           var tool = new ToolViewModel { Id = "Tool1", Title = "Tool1" };

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = CreateList<IDockable>(doc),
                   ActiveDockable = doc
               },
               new ToolDock
               {
                   VisibleDockables = CreateList<IDockable>(tool),
                   ActiveDockable = tool
               });
           return root;
       }
   }

   // Example document view model using INPC
   public class DocumentViewModel : Document
   {
       private string _content = "Document content here...";

       public string Content
       {
           get => _content;
           set => SetProperty(ref _content, value);
       }
   }

   // Example tool view model using INPC
   public class ToolViewModel : Tool
   {
       private string _status = "Ready";

       public string Status
       {
           get => _status;
           set => SetProperty(ref _status, value);
       }
   }
   ```

5. **Create views for your view models**

   Create corresponding views for your documents and tools:

   **DocumentView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.DocumentView">
     <TextBox Text="{Binding Content}" AcceptsReturn="True" />
   </UserControl>
   ```

   **ToolView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.ToolView">
     <StackPanel>
       <TextBlock Text="Tool Panel" FontWeight="Bold" />
       <TextBlock Text="{Binding Status}" />
     </StackPanel>
   </UserControl>
   ```

6. **Initialize the layout**

   Create and initialize the layout in your main window:

   ```csharp
   using Avalonia.Controls;
   using Dock.Avalonia.Controls;
   using MyDockApp.ViewModels;

   namespace MyDockApp;

   public partial class MainWindow : Window
   {
       public MainWindow()
       {
           InitializeComponent();
           InitializeDock();
       }

       private void InitializeDock()
       {
           var factory = new DockFactory();
           var layout = factory.CreateLayout();
           factory.InitLayout(layout);
           
           // Assuming you have a DockControl named "Dock" in MainWindow.axaml
           var dockControl = this.Find<DockControl>("Dock");
           if (dockControl != null)
           {
               dockControl.Layout = layout;
           }
       }
   }
   ```

   And add a `DockControl` to `MainWindow.axaml`:

   ```xaml
   <Window xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           x:Class="MyDockApp.MainWindow"
           Title="My Dock App">
     <DockControl x:Name="Dock" />
   </Window>
   ```

7. **Run the application**

   ```bash
   dotnet run
   ```

## INPC Framework Requirements

When using `Dock.Model.Inpc`, you need to:

1. **Property Change Notifications**: Your view models should inherit from the INPC base classes (`Document`, `Tool`, `RootDock`, etc.) which implement `INotifyPropertyChanged`.

2. **View Locator**: Always set up a view locator to map view models to views.

3. **Factory Setup**: Use `Dock.Model.Inpc.Factory` as your base factory class.

4. **Context and Dockable Locators**: To reattach contexts after loading layouts (and to enable id-based lookups), populate the `ContextLocator` and `DockableLocator` dictionaries in your factory's `InitLayout` method:

   ```csharp
   public override void InitLayout(IDockable layout)
   {
       ContextLocator = new Dictionary<string, Func<object?>>
       {
           ["Doc1"] = () => new DocumentData(),
           ["Tool1"] = () => new ToolData()
       };

       DockableLocator = new Dictionary<string, Func<IDockable?>>
       {
           ["Doc1"] = () => new DocumentViewModel { Id = "Doc1", Title = "Document" },
           ["Tool1"] = () => new ToolViewModel { Id = "Tool1", Title = "Tool1" }
       };

       base.InitLayout(layout);
   }
   ```

## Key Differences from Full MVVM

The INPC implementation provides:

- ✅ Property change notifications via `INotifyPropertyChanged`
- ✅ Basic docking functionality
- ✅ Serialization support
- ✅ Lightweight `SimpleCommand` support for built-in docking commands
- ❌ No advanced MVVM command infrastructure (e.g., ReactiveUI command patterns)

This makes it perfect for scenarios where you want the docking functionality without the overhead of a full MVVM framework, or when integrating with custom MVVM implementations.

For more advanced scenarios and command support, consider the [MVVM guide](dock-mvvm.md) or [ReactiveUI guide](dock-reactiveui.md).
