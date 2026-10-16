# AvalonDock package consumer

This fixture intentionally uses only NuGet references and is outside `Dock.slnx`:
its dependencies must be packed before restoring it. Run from the repository root:

```sh
dotnet pack -c Release -o artifacts/nuget
bash build/test-avalondock-package.sh
```

The same script gates package uploads in CI and releases. It restores into a fresh
cache, maps every `Dock.*` dependency to `artifacts/nuget`, builds .NET 8, and runs
headless xUnit tests on .NET 10. Default settings verify both XAML namespaces,
packaged themes, ordinary resource dictionaries, XML persistence, and explicit
typed registration. A second pass opts into the bundled generator and verifies
automatic application catalog registration of a custom layout type.

The opt-in pass excludes the standalone resource dictionary: generated compiled
resource factories still require an upstream Avalonia API. The default pass keeps
it to catch regressions in the safe default supplied by the packaged build target.
Full WPF parity and the historical fork-based suites remain outside this scope.
