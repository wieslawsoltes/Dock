# AvalonDock compatibility for Dock

This package is under development. It exposes the `Xceed.Wpf.AvalonDock`
namespaces and `http://schemas.xceed.com/wpf/xaml/avalondock` XAML namespace
over `Dock.Model.Avalonia` and Dock's controls. Dock owns the live layout,
selection, dragging, docking, floating, and rendering.

The compatibility target is the public AvalonDock API documented at
https://github.com/xceedsoftware/wpftoolkit/wiki/AvalonDock and the upstream
source at commit `2c71faba5eecc1b6ae6cd3d269408e0df37715d8`.
The implementation is new MIT-licensed adapter code; it does not incorporate
the upstream WPF docking engine.

Reference this package and add the adapter theme alongside your Avalonia theme:

```xml
<Application.Styles>
  <FluentTheme />
  <ad:AvalonDockTheme />
</Application.Styles>
```

Declare `xmlns:ad="http://schemas.xceed.com/wpf/xaml/avalondock"`, then use
`ad:DockingManager`, `ad:LayoutRoot`, `ad:LayoutPanel`, `ad:LayoutDocumentPane`,
`ad:LayoutDocument`, and the corresponding anchorable types in Avalonia XAML.
The runnable `samples/DockAvalonDockSample` contains a licensed adaptation of
the Xceed Live Explorer 3.7.0 sample.

Avalonia applications use Avalonia controls, properties, templates, and
bindings. WPF framework types and compiled WPF assemblies cannot be loaded as
Avalonia controls. Framework type substitutions and any outstanding API or
behavior gaps are tracked in [COMPATIBILITY.md](COMPATIBILITY.md). Full API
compatibility has not yet been established.

## Development validation

```sh
dotnet test tests/Dock.Avalonia.AvalonDock.Tests -c Release
dotnet run --project samples/DockAvalonDockSample
dotnet pack src/Dock.Avalonia.AvalonDock -c Release -o artifacts/nuget
```
