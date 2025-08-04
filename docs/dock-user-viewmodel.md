# Using Your View Models with Dock

Applications often have their own view models that expose domain specific properties. Dock's MVVM helpers let you derive from its `Document`, `Tool` or `Dock` classes so that your models participate in the docking system. This guide explains how to combine your own view models with the Dock view models.

> **💡 Alternative Approach**: Consider using [DocumentDock.ItemsSource](dock-itemssource.md) if you prefer to keep your domain models completely separate from the docking infrastructure. With ItemsSource, you can bind directly to your existing business objects without inheriting from Dock base classes.

## Step-by-step tutorial

1. **Create a custom view model**

   Derive from one of the MVVM base classes and add your own properties. The following example extends `Document`:

   ```csharp
   using Dock.Model.Mvvm.Controls;

   public class FileViewModel : Document
   {
       private string _path = string.Empty;
       public string Path
       {
           get => _path;
           set => SetProperty(ref _path, value);
       }
   }
   ```

2. **Use the view model in a factory**

   Instantiate your custom view models when building the layout.

   ```csharp
   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var file = new FileViewModel { Id = "File1", Title = "File" };

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = CreateList<IDockable>(file),
                   ActiveDockable = file
               });
           root.DefaultDockable = root.VisibleDockables[0];
           return root;
       }
   }
   ```

3. **Initialize the layout**

   In your application, create the factory, build the layout and call `InitLayout`.

   ```csharp
   var factory = new DockFactory();
   var layout = factory.CreateLayout();
   factory.InitLayout(layout);
   Dock.Layout = layout;
   ```

Your custom view models now behave like any other Dock documents or tools while still exposing your application specific state.

## Using `ContextLocator` instead of inheritance

If you prefer to keep your view models independent of Dock you can resolve them through
`ContextLocator`.  Create plain `Document` or `Tool` instances and register the view models
by `Id` when initializing the layout.

```csharp
public class FileInfoViewModel
{
    public string Path { get; set; } = string.Empty;
}

public class DockFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        var document = new Document { Id = "File1", Title = "File" };

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

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            ["File1"] = () => new FileInfoViewModel { Path = "file.txt" }
        };

        base.InitLayout(layout);
    }
}
```

`InitLayout` assigns a new `FileInfoViewModel` to the document's `Context` property so your
view can bind to it.

## Setting `Context` manually

When creating layouts entirely in code you can also bypass `ContextLocator` and assign the
context directly:

```csharp
var document = new Document
{
    Id = "File1",
    Title = "File",
    Context = new FileInfoViewModel { Path = "file.txt" }
};
```

This technique avoids both inheritance and locator dictionaries but does not work with
serialized layouts because the context instance is not recreated automatically.

For further customization options see the [MVVM guide](dock-mvvm.md) and the
[Advanced guide](dock-advanced.md).
