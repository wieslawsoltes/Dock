# Dock Serialization and Persistence

This guide expands on the FAQ and shows how layouts can be serialized and restored using the available serializers.

Dock provides multiple serialization options. Each serializer accepts an optional list type
for dockable collections. If you do not specify one, the serializers default to
`ObservableCollection<>`.

- **`Dock.Serializer.Newtonsoft`** - JSON serialization using Newtonsoft.Json
- **`Dock.Serializer.SystemTextJson`** - JSON serialization using System.Text.Json  
- **`Dock.Serializer.Protobuf`** - Binary serialization using protobuf-net
- **`Dock.Serializer.Xml`** - XML serialization
- **`Dock.Serializer.Yaml`** - YAML serialization

All serializers implement `IDockSerializer` and can be paired with `DockState` to restore document/tool content and document templates that are not serialized. The code snippets below use asynchronous file APIs but any `Stream` works.

## Using JSON serialization (Newtonsoft.Json)

```csharp
using Dock.Serializer;

var serializer = new DockSerializer();
```

To customize list types or use DI-based construction, use the overloads on
`Dock.Serializer.DockSerializer`.

```csharp
using Dock.Serializer;

var serializer = new DockSerializer(typeof(List<>));
var serializerWithDi = new DockSerializer(serviceProvider);
```

## Using JSON serialization (System.Text.Json)

```csharp
using Dock.Serializer.SystemTextJson;

var serializer = new DockSerializer();
```

To customize list types, use the list type overload.

```csharp
using Dock.Serializer.SystemTextJson;

var serializer = new DockSerializer(typeof(List<>));
```

## Using binary serialization (Protobuf)

```csharp
using Dock.Serializer.Protobuf;

var serializer = new ProtobufDockSerializer();
```

To customize list types, use the list type overload.

```csharp
using Dock.Serializer.Protobuf;

var serializer = new ProtobufDockSerializer(typeof(List<>));
```

## Using XML serialization

```csharp
using Dock.Serializer.Xml;

var serializer = new DockXmlSerializer();
```

For XML, you can specify a list type and any known types required for your
custom dockables.

```csharp
using Dock.Serializer.Xml;

var serializer = new DockXmlSerializer(typeof(List<>), typeof(MyDockable));
```

## Using YAML serialization

```csharp
using Dock.Serializer.Yaml;

var serializer = new DockYamlSerializer();
```

To customize list types, use the list type overload.

```csharp
using Dock.Serializer.Yaml;

var serializer = new DockYamlSerializer(typeof(List<>));
```

## Saving a layout

Persist the current layout when the application closes so the user can continue where they left off.

```csharp
await using var stream = File.Create("layout.json");
_serializer.Save(stream, dockControl.Layout!);
```

`dockControl.Layout` must reference the current `IDock` root. The serializer writes JSON, YAML, XML,
or binary data to the stream depending on the implementation you chose.
If you need to preserve document/tool content or document templates, call `_dockState.Save` before saving.

## Loading a layout

When starting the application, load the previously saved layout and restore any cached content or templates.

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dockControl.Layout = layout;
    _dockState.Restore(layout); // restore content/templates
}
```

Before calling `Load`, make sure the assemblies that define your dockable types are loaded. If you need
DI-based construction, use the `Dock.Serializer.DockSerializer` overload that accepts an
`IServiceProvider`.

## Typical pattern

1. Save `dockControl.Layout` with `DockSerializer`.
2. Load the layout and call `_dockState.Restore` to restore content/templates if needed.
3. Handle the case where the layout file does not exist or fails to deserialize.

Following this pattern keeps the window arrangement consistent across sessions.

## Dockable identifiers

Every dockable stores an `Id` string that is written to the layout file. The
value is application-defined and can be used with `ContextLocator` or
`GetDockable` for id-based lookup. When a document dock is split or cloned the
framework copies the original `Id`, so multiple docks can share the same id.
Track your own unique identifier if your application must distinguish
individual document docks.

For an overview of all guides see the [documentation index](README.md).
