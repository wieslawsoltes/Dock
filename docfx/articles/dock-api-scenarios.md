# Dock API Scenarios

This short document collects the most common ways the Dock API is used. The examples apply equally to the MVVM, ReactiveUI, and Avalonia/XAML model variants.

## Dock controls

Dock provides several themed controls that host parts of a layout. These are Avalonia controls used by the built-in themes and data templates.

### DockControl

The main control placed in your view to display a layout. Set the `Layout` and optionally the `Factory` properties from code or XAML:

```xaml
<DockControl Layout="{Binding Layout}"
             Factory="{Binding Factory}"
             InitializeLayout="True"
             InitializeFactory="True" />
```

### DocumentDockControl

Default Avalonia control for `IDocumentDock`. The theme template switches between `DocumentControl` (tabbed) and `MdiDocumentControl` (MDI) based on `LayoutMode`.

### ToolDockControl

Default Avalonia control for `IToolDock`. The theme template hosts `ToolChromeControl` and `ToolControl` to render the active tool and chrome buttons.

## Factories

All runtime operations go through a factory. Pick the implementation that matches your project style:

### `Dock.Model.Avalonia.Factory`

The plain Avalonia variant.  Useful when creating layouts directly in code or XAML without MVVM helpers.

### `Dock.Model.Mvvm.Factory`

Provides base classes implementing `INotifyPropertyChanged`.  Ideal for traditional MVVM view models.

### `Dock.Model.ReactiveUI.Factory`

Wraps the same API with ReactiveUI types. Commands become `ReactiveCommand` and properties derive from `ReactiveObject`.

Other packages expose the same factory pattern, including `Dock.Model.Prism.Factory`, `Dock.Model.CaliburMicro.Factory`, `Dock.Model.Inpc.Factory`, and `Dock.Model.ReactiveProperty.Factory`.

A typical initialization sequence looks like:

```csharp
var factory = new Dock.Model.Mvvm.Factory();
var layout = factory.CreateLayout();
factory.InitLayout(layout);
dockControl.Factory = factory;
dockControl.Layout = layout;
```

These factories expose methods such as `AddDockable`, `MoveDockable` or `FloatDockable` and raise events like `ActiveDockableChanged`. For cross-window state, use `GlobalDockTrackingChanged` and `CurrentDockable`/`CurrentRootDock`.

Use the MVVM, ReactiveUI or XAML samples as references for complete implementations.

### Saving and restoring a layout

Layouts can be persisted using any `IDockSerializer` implementation such as:
`Dock.Serializer.Newtonsoft`, `Dock.Serializer.SystemTextJson`,
`Dock.Serializer.Protobuf`, `Dock.Serializer.Xml`, or `Dock.Serializer.Yaml`.
This allows users to keep their preferred window arrangement between sessions.

```csharp
await using var stream = File.Create(path);
_dockState.Save(layout);
_serializer.Save(stream, layout);
```

Likewise you can restore a saved layout and reinitialize the factory and dock state:

```csharp
await using var stream = File.OpenRead(path);
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dockControl.Layout = layout;
    dockControl.Factory?.InitLayout(layout);
    _dockState.Restore(layout);
}
```

For an overview of all guides see the [documentation index](README.md).
