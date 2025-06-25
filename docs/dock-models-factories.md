# Models and Factories

Dock ships with a set of model libraries that represent the layout tree. `Dock.Model` defines the core interfaces such as `IDock`, `IRootDock` and `IDockable`. The packages `Dock.Model.Mvvm` and `Dock.Model.ReactiveUI` extend these models with MVVM and ReactiveUI friendly base classes.

Factory classes create and manipulate these models at runtime. They provide methods like `CreateRootDock`, `AddDockable` and `FloatDockable` and raise events whenever the layout changes. Choose the factory variant that matches your project style:

- **Dock.Model.Avalonia.Factory** – Plain Avalonia objects, useful when building layouts in code or XAML.
- **Dock.Model.Mvvm.Factory** – Implements `INotifyPropertyChanged` for traditional MVVM view models.
- **Dock.Model.ReactiveUI.Factory** – Uses ReactiveUI types such as `ReactiveObject` and `ReactiveCommand`.

A minimal initialization sequence:

```csharp
var factory = new DockFactory();
var layout = factory.CreateLayout();
factory.InitLayout(layout);
dockControl.Factory = factory;
dockControl.Layout = layout;
```

Factories expose many helpers to move, pin or float dockables. Consult the advanced and API guides for customization points.
