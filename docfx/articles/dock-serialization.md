# Dock Serialization and Persistence

This guide expands on the FAQ and shows how layouts can be serialized and restored using the available serializers.

Dock provides multiple serialization options. Each serializer accepts an optional list type
for dockable collections. If you do not specify one, the serializers default to
`ObservableCollection<>`.

- **`Dock.Serializer.Newtonsoft`** - Compatibility package for the existing `Dock.Serializer.DockSerializer` API, now backed by System.Text.Json
- **`Dock.Serializer.SystemTextJson`** - JSON serialization using System.Text.Json  
- **`Dock.Serializer.Protobuf`** - Binary serialization using protobuf-net
- **`Dock.Serializer.Xml`** - XML serialization
- **`Dock.Serializer.Yaml`** - YAML serialization

All serializers implement `IDockSerializer` and can be paired with `DockState` to restore document/tool content and document templates that are not serialized. The code snippets below use asynchronous file APIs but any `Stream` works.

## Keeping the existing Dock JSON serializer API

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

The default constructor uses generated metadata with no reflection fallback.
It can also load layouts previously written by the Newtonsoft serializer.
To customize list types, use the list type overload.

```csharp
using Dock.Serializer.SystemTextJson;

var serializer = new DockSerializer(typeof(List<>));
```

## Using source-generated JSON serialization (System.Text.Json)

Source generation runs automatically when the serializer package is referenced.
The existing default constructors use generated metadata, so application calls do
not need to change:

```csharp
var existingApi = new Dock.Serializer.DockSerializer();
var jsonApi = new Dock.Serializer.SystemTextJson.DockSerializer();
```

The generator discovers accessible Dock types in the application and referenced
Dock model libraries, types passed to `Serialize`, `Deserialize`, `Save`, or
`Load`, and statically created object-valued payloads. Add an assembly registration
for a payload selected dynamically, for example by a factory or plugin:

```csharp
using Dock.Serializer.SystemTextJson;

[assembly: DockJsonSerializable(typeof(MyTemplatePayload))]
```

`[assembly: DockJsonSourceGeneration]` and the generated
`DockSystemTextJsonGenerated.CreateSerializer()` helper remain supported, but
neither is required by the default constructors. Types must be accessible from
generated code; a private nested type requires an explicitly supplied JSON context
that can access it.

### Previously saved JSON

The default serializers accept Newtonsoft layouts containing assembly-qualified
`$type` names, plain list arrays, `$id`/`$ref` references, and named floating-point
values. The compatibility API also retains data contract member names and its
`IServiceProvider` constructors. Type names are resolved against generated
contracts; deserialization never loads assemblies named in a layout file.

Registered derived types retain their data when saved or loaded through concrete
base types, including shared references. Scalar `object` payloads use the legacy
Newtonsoft value types (for example, integer values load as `long` and fractional
values as `double`) without requiring registrations. Properties removed from the
current contract are ignored, including obsolete nested type names and lists;
`[JsonExtensionData]` members instead retain their raw JSON.

The compatibility API also preserves ordinary public payload fields, Newtonsoft
`[JsonProperty]` member names, `[JsonIgnore]` exclusions, and `JsonObject` opt-in
and field contracts. `[DataMember]` names apply within a `[DataContract]`, matching
Newtonsoft. Generated object handling applies to roots, properties, collection
elements, and dictionary values, preserving registered payload types and shared
references. Collection descriptors follow their generic interfaces, including
sets, queues, read-only collection interfaces, and custom list subclasses. Arrays
used directly in serializer calls are discovered automatically. Untyped JSON
objects and arrays without a CLR type discriminator are retained as `JsonElement`.

Collection element and dictionary value contracts are discovered recursively.
The compatibility API preserves `[DataMember]` fields and properties, including
member names and default-value settings. Nonpublic members use generated
`UnsafeAccessor` methods, which require .NET 8 or later (.NET 9 for members declared
on generic types). On older targets, make those getters/setters accessible to the
generated code; unsupported access fails explicitly instead of dropping saved values.

Newly saved layouts use the System.Text.Json reference-preserving format. Existing
layout files do not require conversion, but older Newtonsoft-based app versions
are not guaranteed to read newly saved files.

The serializers use compiled factories for `List<>`, `ObservableCollection<>`,
and accessible custom list types statically passed using `typeof(MyList<>)`.
Dynamically selected types still need generated metadata; the default path fails
with a descriptive exception rather than falling back to reflection. AOT safety
of custom constructors and converters remains the application's responsibility.

The source-generated path:

- Auto-discovers concrete Dock-derived types in the current compilation and referenced Dock model libraries.
- Includes accessible Dock types from referenced class libraries that reference Dock model contracts.
- Accepts `[assembly: DockJsonSerializable(typeof(...))]` for dynamically selected object-valued payloads, such as custom template content.
- Uses System.Text.Json reference metadata, including the `$type` discriminator and ignored command members.
- Fails fast for unregistered object payloads instead of falling back to reflection.

For advanced composition scenarios, the serializer also exposes resolver-based
constructors:

```csharp
using System.Text.Json.Serialization.Metadata;
using Dock.Serializer.SystemTextJson;

IJsonTypeInfoResolver resolver = new DockSystemTextJsonResolver();

var serializer = new DockSerializer(resolver);
var serializerWithList = new DockSerializer(typeof(List<>), resolver);
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

Before calling `Load`, reference the assemblies that define your dockable types so their metadata is generated. If you need
DI-based construction, use the `Dock.Serializer.DockSerializer` overload that accepts an
`IServiceProvider`.

## Typical pattern

1. Save `dockControl.Layout` with `DockSerializer`.
2. Load the layout and call `_dockState.Restore` to restore content/templates if needed.
3. Handle the case where the layout file does not exist or fails to deserialize.

Following this pattern keeps the window arrangement consistent across sessions. Floating window bounds, title, and `WindowState` are serialized as part of `IDockWindow`. Dockables also persist `DockingState` (`DockingWindowState` flags), so state-aware UI can restore pinned/document/floating/hidden metadata consistently.

## Dockable identifiers

Every dockable stores an `Id` string that is written to the layout file. The
value is application-defined and can be used with `ContextLocator` or
`GetDockable` for id-based lookup. When a document dock is split or cloned the
framework copies the original `Id`, so multiple docks can share the same id.
Track your own unique identifier if your application must distinguish
individual document docks.

For an overview of all guides see the [documentation index](README.md).
