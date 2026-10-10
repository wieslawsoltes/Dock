# NuGet consumer smoke test

This project intentionally uses package references only and is excluded from
the solution: the package must be built before it can restore. It verifies
that a real consumer can compile the default Avalonia XML namespace, render
Dock, apply compiled container styles, content templates and tab/auto-hide
headers, execute container commands, and restore application content from XML
using the packaged assemblies.

```sh
dotnet pack src/Dock.Avalonia.AvalonDock -c Release -o artifacts/nuget
dotnet restore tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  -p:RestoreAdditionalProjectSources="$PWD/artifacts/nuget"
dotnet build tests/Dock.Avalonia.AvalonDock.PackageSmoke -c Release --no-restore
dotnet run --project tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  -c Release -f net10.0 --no-build --no-restore
```

Override `DockCompatibilityPackageVersion` when testing a prerelease package.
Both target frameworks are compiled; the executable smoke test uses .NET 10.
