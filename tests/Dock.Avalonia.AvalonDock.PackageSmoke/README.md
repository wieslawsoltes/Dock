# NuGet consumer smoke test

This project intentionally uses package references only and is excluded from
the solution: the package must be built before it can restore. It verifies
that a real consumer can compile the default Avalonia XML namespace, render
Dock, apply compiled container styles, content templates and tab/auto-hide
headers, render docked/floating tool titles and select their templates, click
native tabs and custom manager menus to execute container commands, and restore
application content from XML using the packaged assemblies. Pack the native
control package too: it contains the tool-menu lifetime fix. An isolated cache
and package-source mapping ensure these local packages are tested even when the
same version exists on nuget.org or in the normal package cache.

```sh
dotnet pack src/Dock.Avalonia.AvalonDock -c Release -o artifacts/nuget
dotnet pack src/Dock.Avalonia -c Release --no-build -o artifacts/nuget
dock_consumer_validation=$(mktemp -d)
cat > "$dock_consumer_validation/NuGet.Config" <<EOF
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$PWD/artifacts/nuget" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="Dock.Avalonia.AvalonDock" />
      <package pattern="Dock.Avalonia" />
    </packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
EOF
dotnet restore tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  --configfile "$dock_consumer_validation/NuGet.Config" \
  --packages "$dock_consumer_validation/packages"
dotnet build tests/Dock.Avalonia.AvalonDock.PackageSmoke -c Release --no-restore
dotnet run --project tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  -c Release -f net10.0 --no-build --no-restore
```

Override `DockCompatibilityPackageVersion` when testing a prerelease package.
Both target frameworks are compiled; the executable smoke test uses .NET 10.
