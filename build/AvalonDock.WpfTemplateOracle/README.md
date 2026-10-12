# WPF template-property reference probe

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
