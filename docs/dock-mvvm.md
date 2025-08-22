# Dock MVVM Guide

This guide explains how to get started with the MVVM version of Dock and describes the available features. It assumes you already have a basic Avalonia application.

The MVVM helpers provided by `Dock.Model.Mvvm` add `INotifyPropertyChanged`
support to the core interfaces and expose a `Factory` base class that wires up
commands and events for you.  The sample project `DockMvvmSample` in the
repository shows a full implementation. For a breakdown of interfaces such as
`IDockable` or `IRootDock` see the [Dock API Reference](dock-reference.md).

> **💡 Modern Approach**: For easier document management, consider using [DocumentDock.ItemsSource](dock-itemssource.md) which automatically creates and manages documents from collections. This approach is covered in detail in the [Document and Tool Content Guide](dock-content-guide.md).

## Step-by-step tutorial

The following steps walk you through creating a very small application that uses Dock with the MVVM helpers.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the Dock packages**

   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Mvvm
   dotnet add package Dock.Avalonia.Themes.Fluent
   ```

   **Optional packages:**
   ```powershell
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   
   # For dependency injection:
   dotnet add package Dock.Model.Extensions.DependencyInjection
   ```

3. **Set up View Locator (Required)**

   Dock requires a view locator to map view models to their corresponding views. Choose one of the following approaches:

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
   using CommunityToolkit.Mvvm.ComponentModel;
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

   **Option A: Traditional Factory Approach**

   Derive from `Dock.Model.Mvvm.Factory` and build the layout by composing docks. Documents and tools should derive from the MVVM versions of `Document` and `Tool`.

   ```csharp
   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var document = new DocumentViewModel { Id = "Doc1", Title = "Document" };
           var documents = CreateList<IDockable>(document);

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = documents,
                   ActiveDockable = document
               });
           root.DefaultDockable = root.VisibleDockables[0];
           return root;
       }
   }
   ```

   **Option B: ItemsSource Approach (Recommended for document management)**

   For dynamic document management, you can also use the ItemsSource approach with MVVM. Create a simple data model and let DocumentDock manage the documents automatically:

   ```csharp
   // Simple data model (no need to inherit from Document)
   public class FileDocumentModel : INotifyPropertyChanged
   {
       private string _title = "";
       private string _content = "";

       public string Title
       {
           get => _title;
           set => SetProperty(ref _title, value);
       }

       public string Content
       {
           get => _content;
           set => SetProperty(ref _content, value);
       }

       public bool CanClose { get; set; } = true;

       // INotifyPropertyChanged implementation...
   }

   // Main ViewModel  
   public class MainViewModel : INotifyPropertyChanged
   {
       public ObservableCollection<FileDocumentModel> Documents { get; } = new();
       
       // Add commands as needed...
   }
   ```

   Then use XAML with ItemsSource:

   ```xaml
   <DockControl>
       <DocumentDock ItemsSource="{Binding Documents}">
           <DocumentDock.DocumentTemplate>
               <DocumentTemplate>
                   <StackPanel Margin="10" x:DataType="Document">
                       <TextBox Text="{Binding Title}" Margin="0,0,0,10"/>
                       <TextBox Text="{Binding Context.Content}" AcceptsReturn="True" Height="300"/>
                   </StackPanel>
               </DocumentTemplate>
           </DocumentDock.DocumentTemplate>
       </DocumentDock>
   </DockControl>
   ```

   > **💡 Tip**: The ItemsSource approach (Option B) provides cleaner separation between your business models and the docking infrastructure, while still working perfectly with MVVM patterns.

4. **Initialize the layout in your main view model**

   ```csharp
   _factory = new DockFactory();
   Layout = _factory.CreateLayout();
   _factory.InitLayout(Layout);
   ```

   `InitLayout` configures services such as `ContextLocator` and
   `DockableLocator` which the view models use to resolve their
   corresponding views. Override this method in your factory if you need
   to register additional mappings or perform custom initialization logic.

5. **Add `DockControl` to `MainWindow.axaml`**

   ```xaml
   <DockControl x:Name="Dock" Layout="{Binding Layout}" />
   ```

6. **Run the project**

   ```bash
   dotnet run
   ```

## Installing

Add the packages for Dock and the MVVM model to your project:

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.Mvvm
Install-Package Dock.Avalonia.Themes.Fluent
```

## Creating a layout

You usually derive from `Factory` and build the layout by composing docks. The sample `DockFactory` shows how tools and documents are created and added to a root layout. More customization options such as overriding `CreateWindowFrom` are covered in the [Advanced Guide](dock-advanced.md):

```csharp
public override IRootDock CreateLayout()
{
    var doc1 = new DocumentViewModel { Id = "Document1", Title = "Document1" };
    var tool1 = new Tool1ViewModel { Id = "Tool1", Title = "Tool1" };

    var root = CreateRootDock();
    root.VisibleDockables = CreateList<IDockable>(
        new DocumentDock
        {
            VisibleDockables = CreateList<IDockable>(doc1),
            ActiveDockable = doc1
        },
        new ToolDock
        {
            VisibleDockables = CreateList<IDockable>(tool1),
            ActiveDockable = tool1,
            Alignment = Alignment.Left
        }
    );
    return root;
}
```

## Docking operations

`FactoryBase` exposes many methods to manipulate the layout at runtime. Some of the most useful ones are:

- `AddDockable`, `InsertDockable` and `RemoveDockable` to manage content.
- `MoveDockable` and `SwapDockable` for rearranging items.
- `PinDockable`/`UnpinDockable` to keep tools in the pinned area.
- `FloatDockable` to open a dockable in a separate window.
- Commands such as `CloseDockable`, `CloseOtherDockables` or `CloseAllDockables`.

```csharp
// Example: add a new document at runtime
var newDoc = new DocumentViewModel { Id = "Doc2", Title = "Another" };
documentDock.AddDocument(newDoc);
```

To drive a "New" command you can assign a delegate to the
`DocumentFactory` property. The built-in `CreateDocument` command will
invoke this factory, pass the result to `AddDocument` and activate the
new document.

## Docking groups

You can use docking groups to control which dockables can be docked together. This is useful for creating isolated areas where only specific types of content can be placed.

```csharp
// Documents can only dock with other documents
var doc1 = new DocumentViewModel 
{ 
    Id = "Doc1", 
    Title = "Document 1",
    DockGroup = "Documents" 
};

// Tools can only dock with other tools
var tool1 = new ToolViewModel 
{ 
    Id = "Tool1", 
    Title = "Tool 1",
    DockGroup = "Tools" 
};

// This tool can dock anywhere (no restrictions)
var flexibleTool = new ToolViewModel 
{ 
    Id = "FlexTool", 
    Title = "Flexible Tool",
    DockGroup = null 
};
```

For a complete guide on docking groups see [Docking Groups](dock-docking-groups.md).

## Events

`FactoryBase` also publishes events for most actions. They allow you to observe changes in active dockable, pinned state or window management.

```csharp
_factory.DockableAdded += (_, e) => Console.WriteLine($"Added {e.Dockable?.Id}");
```

## Next steps

Use the MVVM sample as a starting point for your application. You can extend the factory to create custom docks, documents and tools.
See the [Advanced Guide](dock-advanced.md) for details on overriding factory methods and consult the [Dock API Reference](dock-reference.md) for a complete list of interfaces.

For an overview of all guides see the [documentation index](README.md).
