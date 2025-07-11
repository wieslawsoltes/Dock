# Dock API Scenarios

This short document collects the most common ways the Dock API is used.  The examples apply equally to the MVVM, ReactiveUI and XAML approaches.

## Dock controls

Dock provides several themed controls that host parts of a layout.

### DockControl

The main control placed in your view to display a layout.  Set the `Layout` and optionally the `Factory` properties from code or XAML:

```xaml
<DockControl Layout="{Binding Layout}"
             InitializeLayout="True"
             InitializeFactory="True">
    <DockControl.Factory>
        <Factory />
    </DockControl.Factory>
</DockControl>
```

### DocumentDockControl

Displayed inside a `DocumentDock` to render documents.  The control automatically binds to the active document and shows the `DocumentControl` template.

### ToolDockControl

Used within a `ToolDock` to host tools or side bars.  This control handles layout grip behaviour and interacts with `PinnedDockControl` when tools are pinned or auto-hidden.

## Factories

All runtime operations go through a factory.  Pick the implementation that matches your project style:

### `Dock.Model.Avalonia.Factory`

The plain Avalonia variant.  Useful when creating layouts directly in code or XAML without MVVM helpers.

### `Dock.Model.Mvvm.Factory`

Provides base classes implementing `INotifyPropertyChanged`.  Ideal for traditional MVVM view models.

### `Dock.Model.ReactiveUI.Factory`

Wraps the same API with ReactiveUI types.  Commands become `ReactiveCommand` and properties derive from `ReactiveObject`.

A typical initialization sequence looks like:

```csharp
var factory = new DockFactory();
var layout = factory.CreateLayout();
factory.InitLayout(layout);
dockControl.Factory = factory;
dockControl.Layout = layout;
```

These factories expose methods such as `AddDockable`, `MoveDockable` or `FloatDockable` and raise events like `ActiveDockableChanged` so you can react to changes.

Use the MVVM, ReactiveUI or XAML samples as references for complete implementations.

### Saving and restoring a layout

Layouts can be persisted using `DockSerializer` from the `Dock.Serializer.Newtonsoft` package.
Binary serialization is available via `ProtobufDockSerializer` in
the `Dock.Serializer.Protobuf` package.
This allows users to keep their preferred window arrangement between sessions.

```csharp
await using var stream = File.Create(path);
_serializer.Save(layout, stream);
```

Likewise you can restore a saved layout and reinitialise the factory:

```csharp
await using var stream = File.OpenRead(path);
var layout = _serializer.Load<IDock>(stream);
factory.InitLayout(layout);
dockControl.Layout = layout;
```

For an overview of all guides see the [documentation index](README.md).

