# Dock ReactiveUI Guide

This document mirrors the MVVM instructions for projects that use ReactiveUI.
The API surface is the same but the view models derive from ReactiveUI types and
commands are implemented using `ReactiveCommand`. The sample project
`DockReactiveUISample` demonstrates these concepts in a working application. For
interface details refer to the [Dock API Reference](dock-reference.md) and see
the [Advanced Guide](dock-advanced.md) for more customization options.

## Step-by-step tutorial

Follow these instructions to create a minimal ReactiveUI based application using Dock.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the Dock packages**

   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.ReactiveUI
   ```

3. **Create a factory and view models**

   Derive from `Dock.Model.ReactiveUI.Factory` and implement `CreateLayout`. Your documents and tools should inherit from the ReactiveUI versions of `Document` and `Tool`.

   ```csharp
   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var doc = new DocumentViewModel { Id = "Doc1", Title = "Document" };
           var tool = new Tool1ViewModel { Id = "Tool1", Title = "Tool1" };

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
   ```

4. **Initialize the layout using Reactive commands**

   ```csharp
   _factory = new DockFactory();
   Layout = _factory.CreateLayout();
   _factory.InitLayout(Layout);
   ```

5. **Add `DockControl` to the main view**

   ```xaml
   <DockControl x:Name="Dock" Layout="{Binding Layout}" />
   ```

6. **Run the application**

   ```bash
   dotnet run
   ```

## Installing

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.ReactiveUI
```

## Creating a layout

Create a factory exactly as with the MVVM version. The `DockFactory` in the ReactiveUI sample constructs the layout in the same way.

```csharp
public override IRootDock CreateLayout()
{
    var document1 = new DocumentViewModel { Id = "Document1", Title = "Doc1" };
    var tool1 = new Tool1ViewModel { Id = "Tool1", Title = "Tool1" };

    var root = CreateRootDock();
    root.VisibleDockables = CreateList<IDockable>(
        new DocumentDock
        {
            VisibleDockables = CreateList<IDockable>(document1),
            ActiveDockable = document1
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

The feature set matches the MVVM version. Methods like `AddDockable`, `MoveDockable`, `PinDockable` or `FloatDockable` are available from `FactoryBase`.

```csharp
// Example: create a command that opens a new document
OpenDocument = ReactiveCommand.Create(() =>
{
    var doc = new DocumentViewModel { Id = Guid.NewGuid().ToString(), Title = "New" };
    documentDock.AddDocument(doc);
});
```

Similarly you can set the `DocumentFactory` or `DocumentFactoryAsync`
property so that the dock creates new documents when its `CreateDocument`
command executes. The delegate should return an `IDockable` which is
then passed to `AddDocument` or `AddDocumentAsync` and activated.

## Events

All the events shown in the MVVM guide are present here as well. Subscribe to them in the same way using ReactiveUI commands or observables.

Use the ReactiveUI sample as a template when building your own layouts.
See the [Advanced Guide](dock-advanced.md) for details on customizing factory methods and consult the [Dock API Reference](dock-reference.md) for the available interfaces.

For an overview of all guides see the [documentation index](README.md).
