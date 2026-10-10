# AvalonDock API inventory

This development tool uses Roslyn syntax trees to inventory declared public and
protected types and members without building or loading WPF assemblies. Run it
from the repository root; commands and the pinned source revision are in
`src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md`.

`snapshot` writes sorted type/member keys and signature text. `compare` reports
missing keys and returns 1 if any are missing, 0 otherwise, or 2 for invalid
arguments. A key includes the declaring type, member kind and name, and method
parameter types. It is not a semantic signature or behavioral compatibility
checker. Source aliases, fully qualified names, generic parameter spellings,
framework types, and inherited members require subsequent review. Compile and
exercise representative C# and XAML consumers in addition to this inventory.

The inventory is API metadata. No upstream method bodies, field initializers,
resources, or implementation are copied into the compatibility library.
