# Dock MVVM Guide

This guide explains how to get started with the MVVM version of Dock and describes the available features. It assumes you already have a basic Avalonia application.

The MVVM helpers provided by `Dock.Model.Mvvm` add `INotifyPropertyChanged`
support to the core interfaces and expose a `Factory` base class that wires up
commands and events for you.  The sample project `DockMvvmSample` in the
repository shows a full implementation. For a breakdown of interfaces such as
`IDockable` or `IRootDock` see the [Dock API Reference](dock-reference.md).

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
   ```

3. **Create a factory and view models**

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

## Events

`FactoryBase` also publishes events for most actions. They allow you to observe changes in active dockable, pinned state or window management.

```csharp
_factory.DockableAdded += (_, e) => Console.WriteLine($"Added {e.Dockable?.Id}");
```

## Next steps

Use the MVVM sample as a starting point for your application. You can extend the factory to create custom docks, documents and tools.
See the [Advanced Guide](dock-advanced.md) for details on overriding factory methods and consult the [Dock API Reference](dock-reference.md) for a complete list of interfaces.

For an overview of all guides see the [documentation index](README.md).
