# Dock ReactiveProperty Getting Started Guide

This guide explains how to get started with the ReactiveProperty implementation of Dock. ReactiveProperty is a powerful MVVM framework that provides reactive properties, collections, and commands with excellent performance and validation support.

The `Dock.Model.ReactiveProperty` package integrates Dock with the ReactiveProperty framework. The sample project `DockReactivePropertySample` in the repository demonstrates this approach. For interface details refer to the [Dock API Reference](dock-reference.md).

> **💡 Modern Approach**: For easier document management, consider using [DocumentDock.ItemsSource](dock-itemssource.md) which automatically creates and manages documents from collections. The ItemsSource approach works seamlessly with ReactiveProperty's `ObservableCollection` and reactive commands. This approach is covered in detail in the [Document and Tool Content Guide](dock-content-guide.md).

## Step-by-step tutorial

Follow these instructions to create a ReactiveProperty-based application using Dock.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the Dock packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.ReactiveProperty
   dotnet add package Dock.Avalonia.Themes.Fluent
   dotnet add package ReactiveProperty
   ```

   **Optional packages:**
   ```bash
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   
   # For dependency injection:
   dotnet add package Dock.Model.Extensions.DependencyInjection
   
   # Additional ReactiveProperty features:
   dotnet add package ReactiveProperty.WPF               # For validation attributes
   ```

3. **Set up View Locator (Required)**

   ReactiveProperty requires a view locator to map view models to their corresponding views. Choose one of the following approaches:

   **Option A: Static View Locator with Source Generators (Recommended)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using Dock.Model.ReactiveProperty.Core;
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
           return data is ReactiveBase || data is IDockable;
       }
   }
   ```

   **Option B: Convention-Based View Locator**

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using Dock.Model.ReactiveProperty.Core;

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
           return data is ReactiveBase || data is IDockable;
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

   Derive from `Dock.Model.ReactiveProperty.Factory` and implement `CreateLayout`. Your documents and tools should inherit from the ReactiveProperty versions:

   ```csharp
   using System.Collections.Generic;
   using System.Reactive.Disposables;
   using Dock.Model.Core;
   using Dock.Model.ReactiveProperty;
   using Dock.Model.ReactiveProperty.Controls;
   using Reactive.Bindings;
   using Reactive.Bindings.Extensions;

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

   // Example document view model using ReactiveProperty
   public class DocumentViewModel : Document
   {
       public ReactivePropertySlim<string> Content { get; }
       public ReactiveCommand SaveCommand { get; }

       public DocumentViewModel()
       {
           Content = new ReactivePropertySlim<string>("Document content here...")
               .AddTo(Disposables);

           SaveCommand = new ReactiveCommand()
               .WithSubscribe(() => 
               {
                   // Save logic here
                   System.Diagnostics.Debug.WriteLine($"Saving document: {Title}");
               })
               .AddTo(Disposables);
       }
   }

   // Example tool view model using ReactiveProperty
   public class ToolViewModel : Tool
   {
       public ReactivePropertySlim<string> Status { get; }
       public ReactiveCommand RefreshCommand { get; }

       public ToolViewModel()
       {
           Status = new ReactivePropertySlim<string>("Ready")
               .AddTo(Disposables);

           RefreshCommand = new ReactiveCommand()
               .WithSubscribe(() => 
               {
                   Status.Value = "Refreshed at " + System.DateTime.Now.ToString("HH:mm:ss");
               })
               .AddTo(Disposables);
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
     <DockPanel>
       <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="5">
         <Button Content="Save" Command="{Binding SaveCommand}" />
       </StackPanel>
       <TextBox Text="{Binding Content.Value}" AcceptsReturn="True" />
     </DockPanel>
   </UserControl>
   ```

   **ToolView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.ToolView">
     <StackPanel Margin="5">
       <TextBlock Text="Tool Panel" FontWeight="Bold" Margin="0,0,0,10" />
       <TextBlock Text="{Binding Status.Value}" Margin="0,0,0,5" />
       <Button Content="Refresh" Command="{Binding RefreshCommand}" />
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

## ReactiveProperty Framework Requirements

When using `Dock.Model.ReactiveProperty`, you need to:

1. **Reactive Properties**: Use `ReactiveProperty<T>` or `ReactivePropertySlim<T>` for bindable properties with validation and change notification support.

2. **Commands**: Use `ReactiveCommand` for user actions with automatic can-execute logic.

3. **Disposables**: Properly manage disposables using `CompositeDisposable` and the `AddTo()` extension method.

4. **View Locator**: Always set up a view locator to map view models to views.

5. **Factory Setup**: Use `Dock.Model.ReactiveProperty.Factory` as your base factory class.

6. **Context and Dockable Locators**: For serialization support, populate the locator dictionaries:

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

## ReactiveProperty Features

ReactiveProperty provides several advantages:

- ✅ **Reactive Properties**: Properties with built-in change notifications and validation
- ✅ **Reactive Commands**: Commands with automatic can-execute logic based on reactive properties
- ✅ **Validation**: Built-in validation support with data annotations
- ✅ **Performance**: Efficient property change notifications
- ✅ **Collections**: Reactive collections with change notifications
- ✅ **LINQ**: LINQ-style operators for reactive programming

Example of validation:

```csharp
public class DocumentViewModel : Document
{
    public ReactiveProperty<string> Title { get; }
    public ReactiveProperty<string> Content { get; }
    public ReactiveCommand SaveCommand { get; }

    public DocumentViewModel()
    {
        Title = new ReactiveProperty<string>()
            .SetValidateAttribute(() => Title)  // Use data annotations
            .AddTo(Disposables);

        Content = new ReactiveProperty<string>("Document content...")
            .AddTo(Disposables);

        SaveCommand = new ReactiveCommand(
            Title.ObserveHasErrors.Select(x => !x))  // Enable when no validation errors
            .WithSubscribe(() => Save())
            .AddTo(Disposables);
    }

    private void Save()
    {
        // Save logic
    }
}
```

For more advanced scenarios, see the [Complex layout tutorials](dock-complex-layouts.md) and other ReactiveProperty integration patterns.
