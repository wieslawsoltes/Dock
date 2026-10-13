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

[`PORTABILITY.md`](PORTABILITY.md) classifies the current 120 unmatched
declarations, with an individual mapping in `portability-review.tsv`. The strict
comparison still reports those mismatches; the review is separate evidence.
[`SIGNATURES.md`](SIGNATURES.md) reviews declaration shapes, named arguments,
accessibility, modifiers, generic constraints, and differing base lists for
matching keys. It also records the framework substitutions and behavioral work
that a source declaration review cannot establish.

The adapter intentionally uses `AvalonDock.*` CLR namespaces instead of
upstream's `Xceed.Wpf.AvalonDock.*`. The snapshot retains the original upstream
names. Comparison applies this namespace mapping to keys on both sides; it
does not exclude any upstream declarations. Both source namespaces are accepted
so the pinned upstream source can still be used for the 951/951 self-check.

Comparison also canonicalizes the known BCL type `CultureInfo` when it is written
as `System.Globalization.CultureInfo` or `global::System.Globalization.CultureInfo`.
This corrects qualification-only differences in converter signatures without
changing the snapshot. It does not erase arbitrary namespaces or equate WPF
framework types with Avalonia types. Tests exercise the shared comparison helper.
Array rank spacing is also canonicalized: stripping a nullable element annotation
from Roslyn's `object? []` rendering must produce the same key as `object[]`.

The inventory is API metadata. No upstream method bodies, field initializers,
resources, or implementation are copied into the compatibility library.
