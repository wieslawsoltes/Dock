# Source-Generated `System.Text.Json` Serializer for Dock

## Summary
- Create `plan/system-text-json-source-generator-plan.md` first and write this plan into it verbatim.
- Keep the current `Dock.Serializer.SystemTextJson.DockSerializer()` behavior unchanged as the compatibility path.
- Add an opt-in source-generated path inside the same package by introducing a Roslyn incremental generator project and new resolver-based `DockSerializer` constructors.
- Support user types in two tiers:
  - Auto-discover concrete Dock-derived types in the consuming compilation.
  - Require explicit assembly attributes for extra `object` payload types and referenced-library Dock types.

## Implementation Changes
- Add a new analyzer project `Dock.Serializer.SystemTextJson.Generators` targeting `netstandard2.0` and using `IIncrementalGenerator`.
- Add two runtime attributes in `Dock.Serializer.SystemTextJson`:
  - `DockJsonSourceGenerationAttribute` as an assembly-level activation switch.
  - `DockJsonSerializableAttribute(Type type)` as an assembly-level registration mechanism for extra payload types and referenced-library types.
- Extend `Dock.Serializer.SystemTextJson.DockSerializer` with two new public constructors:
  - `DockSerializer(IJsonTypeInfoResolver typeInfoResolver)`
  - `DockSerializer(Type listType, IJsonTypeInfoResolver typeInfoResolver)`
- Extract shared `JsonSerializerOptions` creation so both the reflection path and generated path use the same defaults:
  - `WriteIndented = true`
  - `ReferenceHandler = Preserve`
  - `DefaultIgnoreCondition = WhenWritingNull`
  - `NumberHandling = AllowNamedFloatingPointLiterals`
  - existing `JsonConverterFactoryList`
- Leave the current reflection resolver in place for the old constructors; do not chain reflection fallback into the generated path.
- In the generator, discover these Dock contracts once from the compilation: `IDockable`, `IDock`, `IRootDock`, `IDockWindow`, `IDocumentTemplate`, `IToolTemplate`.
- Auto-discover only current-compilation, concrete, non-abstract, closed types assignable to those contracts.
- Read repeated `[assembly: DockJsonSerializable(typeof(...))]` attributes and include those types even when they come from referenced assemblies.
- Emit generated code into the consumer assembly with stable names:
  - `DockSystemTextJsonContext.g.cs` containing a `JsonSerializerContext` with `[JsonSerializable]` entries for Dock root contracts, required collection contracts, discovered Dock types, and explicitly registered extra types.
  - `DockSystemTextJsonGenerated.g.cs` containing:
    - an internal resolver that serves metadata only from the generated context,
    - compile-time-generated polymorphism tables for Dock interfaces,
    - compile-time-generated ignored-member tables derived from `IgnoreDataMember` and `ICommand` properties,
    - an internal helper `CreateSerializer()` and `CreateSerializer(Type listType)`.
- Preserve wire compatibility with the current serializer by keeping:
  - discriminator property name as `$type`
  - discriminator value as `Type.FullName`
  - same ignored-member behavior for commands and `IgnoreDataMember` members
- For `object`-typed payloads such as `Context` and template `Content`, configure polymorphism only for explicitly registered extra types. Unregistered payloads should fail in the generated path instead of silently reflecting.
- Emit generator diagnostics with a stable prefix `DSTJ`:
  - invalid registered type
  - activation attribute missing when extra registration attributes are present
  - duplicate discriminator collision from identical full names
  - missing Dock symbols in an activated project
- Wire solution and packaging so the main `Dock.Serializer.SystemTextJson` NuGet ships the generator DLL under `analyzers/dotnet/cs`, and local solution builds reference it as an analyzer.

## Test Plan
- Keep all existing `Dock.Serializer.UnitTests` passing unchanged to prove compatibility for the old constructors.
- Add generator unit tests that verify:
  - activation attribute produces generated sources
  - auto-discovery finds custom Dock-derived types in source
  - explicit registration includes extra payload types
  - duplicate discriminator collisions emit the expected diagnostic
  - invalid/open generic registrations emit the expected diagnostic
- Add end-to-end runtime tests in a dedicated source-gen test project that references the generator as an analyzer and verifies:
  - generated serializer round-trips custom `RootDock`, `Document`, `Tool`, and `DockWindow` subclasses
  - generated serializer round-trips `Context` or template `Content` when the payload type is explicitly registered
  - generated serializer throws for unregistered payload types
  - JSON produced by the generated path can be read by the existing reflection serializer, and vice versa
  - list-type override still deserializes `IList<T>` into the requested concrete list type
- Run targeted `dotnet test` for the existing serializer tests and the new generator/source-gen test projects.

## Assumptions And Defaults
- Plan file path: `plan/system-text-json-source-generator-plan.md`.
- Generation is explicit: consumers must add `[assembly: DockJsonSourceGeneration]` to activate it.
- Auto-discovery is limited to the current compilation to avoid scanning every referenced assembly.
- Referenced-library Dock types and all `object` payload types must be added with `[assembly: DockJsonSerializable(typeof(...))]`.
- The generated path is intentionally reflection-free at runtime; the old parameterless serializer remains the fallback compatibility path.
- Documentation updates should cover usage and registration in `dock-serialization.md` and the FAQ/custom-model guidance.
- Official references used for the design:
  - [System.Text.Json source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
  - [System.Text.Json custom contracts and resolvers](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/custom-contracts)
  - [NuGet analyzer packaging conventions](https://learn.microsoft.com/en-us/nuget/guides/analyzers-conventions)
