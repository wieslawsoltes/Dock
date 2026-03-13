# PR Summary: Source-Generated `System.Text.Json` Serializer for Dock

## Overview

This change adds an opt-in source-generated `System.Text.Json` serializer path for Dock while preserving the existing reflection-based `Dock.Serializer.SystemTextJson.DockSerializer()` behavior as the compatibility default.

The implementation introduces a Roslyn incremental generator that emits consumer-side serializer metadata and runtime helpers, plus new resolver-based `DockSerializer` constructors for explicitly using generated metadata without falling back to reflection.

## Goals

- Keep the existing serializer behavior unchanged for current consumers.
- Add a reflection-free generated path for applications that opt in.
- Support user-defined Dock model types.
- Support cross-assembly custom Dock types and `object` payloads through explicit registration.
- Preserve the existing JSON wire shape so generated and reflection serializers remain compatible.

## Runtime Changes

### New public API

Added two new public constructors to `Dock.Serializer.SystemTextJson.DockSerializer`:

- `DockSerializer(IJsonTypeInfoResolver typeInfoResolver)`
- `DockSerializer(Type listType, IJsonTypeInfoResolver typeInfoResolver)`

These constructors are intended for the generated path and now validate inputs so `null` cannot silently re-enable reflection-based metadata resolution.

### Shared serializer options

Extracted common option construction into `DockSerializerOptionsFactory` so both reflection and generated serializers use the same defaults:

- `WriteIndented = true`
- `ReferenceHandler = Preserve`
- `DefaultIgnoreCondition = WhenWritingNull`
- `NumberHandling = AllowNamedFloatingPointLiterals`
- existing `JsonConverterFactoryList`

### New assembly-level attributes

Added:

- `DockJsonSourceGenerationAttribute`
- `DockJsonSerializableAttribute(Type type)`

These attributes activate the generator and let consumers register additional types for generated metadata.

## Generator Implementation

### New project

Added `src/Dock.Serializer.SystemTextJson.Generators`, a `netstandard2.0` Roslyn incremental generator project.

### Discovery model

The generator:

- auto-discovers concrete, non-abstract, closed Dock-derived types from the current compilation
- recognizes these Dock contracts:
  - `IDockable`
  - `IDock`
  - `IRootDock`
  - `IDockWindow`
  - `IDocumentTemplate`
  - `IToolTemplate`
- reads repeated `[assembly: DockJsonSerializable(typeof(...))]` attributes
- accepts explicitly registered referenced-assembly Dock types and payload types

### Generated outputs

The generator emits:

- `DockSystemTextJsonContext.g.cs`
- `DockSystemTextJsonGenerated.g.cs`

The generated runtime helper includes:

- a resolver over generated `JsonTypeInfo`
- compile-time polymorphism tables for Dock interfaces
- ignored-member handling for `IgnoreDataMember` and `ICommand`
- generated serializer factory helpers
- a custom object-payload converter for explicitly registered object payload types

### Wire compatibility

The generated path preserves:

- discriminator property name: `$type`
- discriminator values: `Type.FullName`
- ignored member behavior for command properties and `IgnoreDataMember`

Generated JSON remains readable by the existing reflection serializer, and reflection-generated JSON remains readable by the generated serializer.

### Diagnostics

Added generator diagnostics:

- `DSTJ001` invalid registration
- `DSTJ002` missing activation attribute
- `DSTJ003` duplicate discriminator collision
- `DSTJ004` missing Dock symbols

### Robustness fixes included during review

The implementation also includes follow-up fixes discovered during review:

- the generated-path constructors now throw on `null` resolvers instead of allowing default reflection resolution
- the generator now accepts valid `protected internal` consumer Dock types during auto-discovery
- `System.Text.Json.SourceGeneration.dll` resolution no longer depends on a hard-coded Roslyn folder list and instead searches the analyzer directory structure

## Packaging

Updated `Dock.Serializer.SystemTextJson.csproj` so the main NuGet package includes:

- `analyzers/dotnet/cs/Dock.Serializer.SystemTextJson.Generators.dll`

This allows consuming projects to get the generator automatically from the main package.

## Test Coverage

### Generator unit tests

Added `tests/Dock.Serializer.SystemTextJson.Generators.UnitTests` covering:

- activation attribute generation
- auto-discovery of custom Dock types
- explicit registration of object payload types
- duplicate discriminator diagnostics
- invalid/open generic registration diagnostics
- auto-discovery of valid `protected internal` nested Dock types

### End-to-end source-generation tests

Added `tests/Dock.Serializer.SystemTextJson.SourceGenTests` covering:

- round-trip serialization of custom Dock-derived types in the consuming assembly
- round-trip serialization of explicitly registered object payloads
- failure for unregistered object payloads
- wire compatibility with the existing reflection serializer
- list-type override behavior
- use of the public resolver-based `DockSerializer` constructor with the generated resolver

### Cross-assembly integration tests

Added `tests/Dock.Serializer.SystemTextJson.SourceGenSharedTypes` to model referenced-library types and payloads, then verified:

- referenced-assembly Dock types are handled when explicitly registered
- referenced-assembly template content payloads are handled when explicitly registered

## Documentation

Updated:

- `docfx/articles/dock-serialization.md`
- `docfx/articles/dock-custom-model.md`
- `docfx/articles/dock-faq.md`

These docs now describe:

- how to opt into source generation
- how to construct the generated serializer
- when explicit registration is required
- why unregistered object payloads fail in the generated path

## Commit Breakdown

1. `170e312e2` `Add source-generated serializer plan`
2. `61a28ef29` `Add source-generated System.Text.Json serializer`
3. `d58ac7d45` `Add source-generated serializer test coverage`
4. `a1c2b8bb9` `Document source-generated serializer usage`

## Validation Performed

Executed successfully:

- `dotnet test tests/Dock.Serializer.SystemTextJson.Generators.UnitTests/Dock.Serializer.SystemTextJson.Generators.UnitTests.csproj -c Debug`
- `dotnet test tests/Dock.Serializer.SystemTextJson.SourceGenTests/Dock.Serializer.SystemTextJson.SourceGenTests.csproj -c Debug`
- `dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Debug`

Also validated packaging with:

- `dotnet pack src/Dock.Serializer.SystemTextJson/Dock.Serializer.SystemTextJson.csproj -c Debug`

The packed nupkg includes the generator assembly under `analyzers/dotnet/cs/`.

## Notes

- The default parameterless `DockSerializer()` remains the compatibility path.
- Source generation is explicit and opt-in.
- Unregistered object payloads intentionally fail at runtime in the generated path instead of silently reflecting.
- Real end-to-end source-generated behavior is validated in a consumer test project, not only via in-memory Roslyn unit tests.
