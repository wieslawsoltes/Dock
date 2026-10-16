# AvalonDock framework work (deferred)

Regular Dock builds and releases use official Avalonia packages. The AvalonDock
compatibility source builds against public Avalonia APIs, and
`samples/DockAvalonDockSample` plus its focused headless tests are included in
`Dock.slnx` and CI. Run `dotnet run --project samples/DockAvalonDockSample`.
The compatibility package is published against official Avalonia with focused
package-consumer validation. Full framework parity, adapter API documentation
generation, and the historical parity suite remain deferred until the required
upstream contracts are available.

[Dock issue #1186](https://github.com/wieslawsoltes/Dock/issues/1186) tracks all
required contracts, migration steps, validation, and full-parity acceptance:

- Opt-in property metadata for WPF-compatible binding ownership and current values.
- Logical-child projections and weak inheritance with correct lifetime propagation.
- Hosted native roots for auto-hide resources, styles, input, focus, and disposal.
- Initialization deferral and primary-pointer capture for floating hosts.
- Reflection-free factories for compiled XAML resources.

The runnable source uses ordinary Avalonia property registrations, public
`ILogical` projections, logical parenting for embedded roots, model-ready
initialization callbacks, and pointer instances received through input events.
These public API paths do not reproduce every WPF binding, lifecycle, inheritance,
or native-host behavior. The sample's tests establish its supported workflows;
the historical full-parity results below do not apply to these substitutes.

The reference implementation is in
[Avalonia fork PR #9](https://github.com/wieslawsoltes/Avalonia/pull/9).
That fork PR is not an upstream AvaloniaUI merge or an official package release.
Reactivation must use the accepted upstream equivalents and verify their
availability in public NuGet packages.

## Historical reproduction

The full prototype was validated at Dock revision
[`447653dbc589757eb6cbc6062dd3ffe0f79629ee`](https://github.com/wieslawsoltes/Dock/tree/447653dbc589757eb6cbc6062dd3ffe0f79629ee),
using framework runtime revision
[`57744b19b6ab8ff05edb3776474792467b9d545b`](https://github.com/wieslawsoltes/Avalonia/commit/57744b19b6ab8ff05edb3776474792467b9d545b)
and custom package version `12.1.4-avalondock.4`, based on Avalonia 12.1.3.
That custom version is distinct from official Avalonia 12.1.4.

Use a separate checkout of the historical Dock revision and its
[original build guide](https://github.com/wieslawsoltes/Dock/blob/447653dbc589757eb6cbc6062dd3ffe0f79629ee/build/AvalonDock.Framework/README.md)
to reproduce those results. The bootstrap, custom feed, framework bundle, and
unpublished-framework publishing gate have been removed from the current build.
Historical prototype test results do not establish compatibility with official
Avalonia packages.
