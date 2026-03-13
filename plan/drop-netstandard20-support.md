# Plan: Drop `netstandard2.0` Support from Dock

## Objective

Remove `netstandard2.0` from Dock runtime library targets and repo runtime packaging, while keeping the supported runtime matrix aligned on `net6.0`, `net8.0`, and `net10.0`.

## Analysis Findings

- `22` source/library projects still targeted `netstandard2.0` alongside modern .NET TFMs.
- `1` Roslyn source-generator project, `Dock.Serializer.SystemTextJson.Generators`, targeted only `netstandard2.0`.
- `3` generator consumer projects pinned the analyzer reference to `TargetFramework=netstandard2.0`.
- `5` source files contained `NETSTANDARD2_0` branches or polyfills, and the generator project carried a dedicated `IsExternalInit` shim.
- `README.md` still advertised `netstandard2.0` as a supported build target.

## Migration Decisions

- Runtime and shipping libraries move to `net6.0;net8.0;net10.0`.
- The `Dock.Serializer.SystemTextJson` source generator remains `netstandard2.0` as a Roslyn compatibility exception. It is a build-time compiler component, not a shipped runtime library target.
- The source-generator analyzer is still packed through `Dock.Serializer.SystemTextJson`, but the package now lifts the analyzer from the generator's `netstandard2.0` output and injects it only once during the `net6.0` pack target.
- `NETSTANDARD2_0` compatibility branches and shims are removed so the codebase uses the modern BCL surface directly.

## Implementation Steps

1. Update all affected library `.csproj` files to remove `netstandard2.0` from `TargetFrameworks`.
2. Keep `Dock.Serializer.SystemTextJson.Generators` on `netstandard2.0` as the analyzer-host compatibility exception.
3. Keep analyzer `ProjectReference` metadata aligned with the generator target while removing `netstandard2.0` from runtime library TFMs.
4. Remove obsolete `NETSTANDARD2_0` code paths and polyfill files.
5. Update repo documentation so the supported framework matrix matches the project files.
6. Validate with restore/build/test/pack using the repository SDK.

## Risk Notes

- Roslyn compiler extensions still expect `netstandard2.0` for maximum host compatibility, so the source generator is intentionally excluded from the runtime-TFM cleanup.
- The AOT sample and source-generation tests are part of the validation surface because they exercise the analyzer `ProjectReference` path directly.

## Validation Results

- `dotnet build Dock.slnx -c Release` succeeded.
- `dotnet test Dock.slnx -c Release --no-build` succeeded.
- `dotnet pack Dock.slnx -c Release --no-build -o artifacts/nuget` succeeded.
- `dotnet publish samples/DockSystemTextJsonSourceGenAotSample/DockSystemTextJsonSourceGenAotSample.csproj -c Release -r osx-arm64 -o artifacts/aot-sourcegen-sample` succeeded.
- `./artifacts/aot-sourcegen-sample/DockSystemTextJsonSourceGenAotSample` succeeded and reported a successful round trip.
- `artifacts/nuget/Dock.Serializer.SystemTextJson.11.3.11.22.nupkg` contains `lib/net6.0`, `lib/net8.0`, `lib/net10.0`, plus `analyzers/dotnet/cs/Dock.Serializer.SystemTextJson.Generators.dll`.

## Analyzer Exception

- `src/Dock.Serializer.SystemTextJson.Generators` remains on `netstandard2.0`. That is an intentional exception so the analyzer/source-generator stays compatible with Roslyn hosts and does not trigger `RS1041`.

## Remaining `netstandard` Strings

- The remaining `netstandard` references in the repository are:
- `src/Dock.Serializer.SystemTextJson.Generators/Dock.Serializer.SystemTextJson.Generators.csproj` and analyzer `ProjectReference` metadata, which are kept for Roslyn compatibility.
- Avalonia XAML XML namespace declarations such as `clr-namespace:System;assembly=netstandard`, which are XAML assembly identifiers, not project TFMs.
