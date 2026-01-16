# Creating a Custom Dock.Model Implementation

Dock ships with a set of factory libraries that adapt the base model to various
MVVM frameworks:

- `Dock.Model.Avalonia` - Plain Avalonia version with minimal dependencies
- `Dock.Model.Mvvm` - MVVM implementation with `INotifyPropertyChanged` helpers
- `Dock.Model.CaliburMicro` - Caliburn.Micro implementation with `PropertyChangedBase`
- `Dock.Model.Inpc` - Basic `INotifyPropertyChanged` implementation without MVVM commands
- `Dock.Model.ReactiveUI` - ReactiveUI integration with observables and commands
- `Dock.Model.ReactiveProperty` - ReactiveProperty framework integration  
- `Dock.Model.Prism` - Prism framework integration with commands and bindings

You can create your own implementation when these do not fit your application or you wish to integrate Dock with another framework.

## Using Dock.Model.Inpc

If you only need basic property change notifications and lightweight `ICommand` implementations without a framework dependency, use `Dock.Model.Inpc`:

```bash
dotnet add package Dock.Model.Inpc
```

This package provides `INotifyPropertyChanged` implementations without the additional overhead of command patterns, making it ideal for simpler scenarios or custom MVVM frameworks. The [DockInpcSample](../samples/DockInpcSample) demonstrates this approach.

## Project setup

1. **Create a new class library** and add a reference to `Dock.Model`.

2. **Set up a view locator (when using DataTemplates)** - If your dockables rely on view model to view mapping, register a view locator:

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;

   public class CustomViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           // Implement your view resolution logic here
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

           var name = data.GetType().FullName!.Replace("ViewModel", "View");
           return Type.GetType(name) is not null;
       }
   }
   ```

   Register it in your App.axaml:
   ```xaml
   <Application.DataTemplates>
     <local:CustomViewLocator />
   </Application.DataTemplates>
   ```

3. **Implement the interfaces** from `Dock.Model.Core` and `Dock.Model.Controls`
   using the base classes that match your framework.  The existing
   implementations in the repository are good starting points.

4. **Derive a Factory** from `Dock.Model.FactoryBase` and override the creation
   methods (`CreateRootDock`, `CreateToolDock`, `CreateDocumentDock` and so on)
   to return your custom view models.

5. **Provide command and property change logic** that matches your MVVM framework.
   For example, you might expose `ICommand` objects or ReactiveUI commands.

A minimal dockable using `INotifyPropertyChanged` might look like this (implement the `IDockable` members you need, or start from `Dock.Model.Inpc.Core.DockableBase` and adapt it):

```csharp
public abstract class MyDockableBase : IDockable, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Implement IDockable members here or copy from Dock.Model.Inpc.Core.DockableBase.
}
```

Your factory can then create instances of `MyDockable` and initialize them using
`InitLayout` just like the built-in MVVM and ReactiveUI versions.  The
interfaces and helper methods in `FactoryBase` remain the same regardless of the
underlying framework.

## Reusing existing code

The easiest way to start is to copy the sources of `Dock.Model.Mvvm` or
`Dock.Model.ReactiveUI` and replace the command and property change types with
those from your own framework.  Only a handful of classes need to be adjusted,
mainly the `Dockable` bases and the `Factory` implementation.

## Conclusion

Custom implementations let you integrate Dock with any MVVM pattern while
keeping the docking logic intact.  Follow the structure of the provided models
and adapt the base classes to match your chosen framework.

For an overview of all guides see the [documentation index](README.md).
