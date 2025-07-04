# Creating a Custom Dock.Model Implementation

Dock ships with a set of factory libraries that adapt the base model to various
MVVM frameworks. `Dock.Model.Avalonia` provides a plain Avalonia version,
`Dock.Model.Mvvm` adds `INotifyPropertyChanged` helpers and
`Dock.Model.ReactiveUI` wraps the same API with ReactiveUI types.  You can create
your own implementation when these do not fit your application or you wish to
integrate Dock with another framework.

## Project setup

1. Create a new class library and add a reference to `Dock.Model`.
2. Implement the interfaces from `Dock.Model.Core` and `Dock.Model.Controls`
   using the base classes that match your framework.  The existing
   implementations in the repository are good starting points.
3. Derive a `Factory` from `Dock.Model.FactoryBase` and override the creation
   methods (`CreateRootDock`, `CreateToolDock`, `CreateDocumentDock` and so on)
   to return your custom view models.
4. Provide command and property change logic that matches your MVVM framework.
   For example, you might expose `ICommand` objects or ReactiveUI commands.

A minimal dockable using `INotifyPropertyChanged` might look like this:

```csharp
public class MyDockable : DockableBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

Your factory can then create instances of `MyDockable` and initialize them using
`InitLayout` just like the built‑in MVVM and ReactiveUI versions.  The
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
