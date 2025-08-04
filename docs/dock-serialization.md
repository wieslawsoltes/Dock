# Dock Serialization and Persistence

This guide expands on the FAQ and shows how layouts can be serialized and restored using the available serializers.

Dock provides multiple serialization options:

- **`Dock.Serializer.Newtonsoft`** - JSON serialization using Newtonsoft.Json
- **`Dock.Serializer.SystemTextJson`** - JSON serialization using System.Text.Json  
- **`Dock.Serializer.Protobuf`** - Binary serialization using protobuf-net
- **`Dock.Serializer.Xml`** - XML serialization
- **`Dock.Serializer.Yaml`** - YAML serialization

All serializers implement `IDockSerializer` and work with `DockState` to track focus changes. The code snippets below use asynchronous file APIs but any `Stream` works.

## Using JSON serialization (Newtonsoft.Json)

```csharp
using Dock.Serializer;

var serializer = new DockSerializer();
```

## Using JSON serialization (System.Text.Json)

```csharp
using Dock.Serializer.SystemTextJson;

var serializer = new DockSerializer();
```

## Using binary serialization (Protobuf)

```csharp
using Dock.Serializer.Protobuf;

var serializer = new ProtobufDockSerializer();
```

## Using XML serialization

```csharp
using Dock.Serializer.Xml;

var serializer = new DockXmlSerializer();
```

## Using YAML serialization

```csharp
using Dock.Serializer.Yaml;

var serializer = new DockYamlSerializer();
```

## Saving a layout

Persist the current layout when the application closes so the user can continue where they left off.

```csharp
await using var stream = File.Create("layout.json");
_serializer.Save(dockControl.Layout!, stream);
```

`dockControl.Layout` must reference the current `IDock` root. The serializer writes JSON to the stream.

## Loading a layout

When starting the application, load the previously saved layout and restore the last active and focused dockables.

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dockControl.Layout = layout;
    _dockState.Restore(layout); // reapply focus information
}
```

Before calling `Load` ensure that custom dockables are registered with `DockableLocator` and `ContextLocator`. Unknown identifiers return `null`.

## Typical pattern

1. Register view models with `DockableLocator` at startup.
2. On application exit, save `dockControl.Layout` with `DockSerializer`.
3. On the next run, load the layout and call `_dockState.Restore`.
4. Handle the case where the layout file does not exist or fails to deserialize.

Following this pattern keeps the window arrangement consistent across sessions.

## Dockable identifiers

Every dockable stores an `Id` string that is written to the layout file. During
deserialization `DockableLocator` receives this identifier and returns the view
model to instantiate. The value is a type key, not a unique runtime ID. When a
document dock is split or cloned the framework copies the original `Id` so each
section resolves to the same view model type. Track your own unique identifier
if your application must distinguish individual document docks.

For an overview of all guides see the [documentation index](README.md).
