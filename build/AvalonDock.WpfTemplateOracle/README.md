# WPF compatibility reference probes

This Windows-only developer tool records public values, binding ownership, and
protected callback ordering from the original WPF AvalonDock. It is separate
from the Dock solution and packages. Supply your own upstream checkout; no
upstream implementation or binaries are included here.

The checked-in observations were measured from upstream commit
`2c71faba5eecc1b6ae6cd3d269408e0df37715d8`, using Windows 11 ARM64 and .NET 10.
The upstream projects keep their original `net5.0-windows` target; the probe
runs against the .NET 10 WPF runtime. Respect the upstream checkout's license.

From a Windows PowerShell shell with .NET 10 installed:

```powershell
$reference = "C:\reference\wpftoolkit\ExtendedWPFToolkitSolution\Src\Xceed.Wpf.AvalonDock\Xceed.Wpf.AvalonDock.NET5.csproj"
dotnet build build/AvalonDock.WpfTemplateOracle -c Release "-p:AvalonDockReferenceProject=$reference"
dotnet build/AvalonDock.WpfTemplateOracle/bin/Release/net10.0-windows/AvalonDock.WpfTemplateOracle.dll wpf-observations.tsv
```

Review the output before replacing
`tests/Dock.Avalonia.AvalonDock.Tests/Assets/WpfTemplateProperties.tsv`.
`TemplatePropertyTests` compares the Avalonia adapter with all 247 observations:
five template/selector pairs, local values, bindings, styles, base callbacks,
subclass suppression, selector replacement/removal, explicit coercion, and the
menu-header fallback. The comparison adapts the external WPF `SetValue` operation
by explicitly disposing its binding before the corresponding Avalonia assignment:
Avalonia's inherited `SetValue` retains bindings. The preceding selector stage
checks the adapter callback's own binding disposal without that adaptation.
The probe uses typed dependency-property references and
bindings without property paths; it does not inspect private framework state.

The floating-close probe uses the same build and records protected hooks plus
actual floating windows:

```powershell
dotnet build/AvalonDock.WpfTemplateOracle/bin/Release/net10.0-windows/AvalonDock.WpfTemplateOracle.dll --closing wpf-closing.tsv
```

`tests/Dock.Avalonia.AvalonDock.Tests/Assets/WpfFloatingClose.tsv` contains all
168 observations: 48 protected-hook combinations and before/synchronous/dispatched
states for 40 document/tool scenarios. These distinguish `Window.Close()`, model
closure, item commands, layout replacement, and view unload, including cancellation.
`FloatingCloseReferenceTests` compares all 48 hook results and all three states of
all 40 host scenarios (168 recorded rows, 88 test cases).
Avalonia's closing arguments have an internal constructor, so the hook comparison
enters through `Window.Close()` and holds the host open after recording the result.

The comparison includes deferred internal closure, retained document roots after
content `Closed`, closed tool hosts remaining in the manager's public collection,
and canceled unloads clearing that collection. `DeferredFloatingCloseTests` also
checks native host/factory cleanup, explicit retries, reattachment, replacement
hosts, and document adoption. These fixtures cover the recorded scenarios;
remaining platform and floating-window behavior still require a broader audit.

The floating-model probe measures replacement and notification overrides:

```powershell
dotnet build/AvalonDock.WpfTemplateOracle/bin/Release/net10.0-windows/AvalonDock.WpfTemplateOracle.dll --floating-models wpf-floating-models.tsv
```

`Assets/WpfFloatingModels.tsv` in the compatibility tests records all 180
observations: before/synchronous/dispatched states for 60 scenarios. Document and
tool windows are cleared, replaced, or cleared and restored before dispatch,
with cancellation and overrides suppressing child, validity, visibility, or all
property notifications. `FloatingModelReferenceTests` compares every row,
including old/new parent and root retention. Document hosts observe the explicit
`RootDocumentChanged` event; tool hosts observe `RootPanel` and `IsVisible`
property notifications. The native adoption and retained-model lifetime tests
also verify that those compatibility rules do not leave stale native ownership
or retain closed hosts.
