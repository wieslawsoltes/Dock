# Compatibility work and acceptance criteria

The objective remains complete AvalonDock C# and XAML API compatibility on
Avalonia, using Dock's Avalonia models and docking implementation, a NuGet
package, and a licensed adaptation of the Xceed sample. This document records
remaining work; a passing subset of tests is not proof of full compatibility.

Reference: Xceed `wpftoolkit` commit
`2c71faba5eecc1b6ae6cd3d269408e0df37715d8`, including public layout types,
interfaces, extension methods, `DockingManager`, controls, commands, themes,
converters, events, and XML serialization. Public WPF framework types require
Avalonia counterparts; binary compatibility with WPF is not possible.

Required verification before declaring completion:

- Public type/member/signature inventory compared to the pinned upstream API,
  with explicit framework substitutions and no unexplained missing members.
- Compiled XAML fixtures for the documented layout hierarchy, attached and
  regular properties, templates, sources, styles, themes, and namespaces.
- Bidirectional collection, property, ownership, selection, and active-content
  synchronization with the actual `Dock.Model.Avalonia` objects, including
  Dock-created panes during dragging and floating.
- Closing cancellation, hiding, restoring, auto-hide, docking as document,
  floating/redocking, size constraints, and update strategies.
- Dynamic document/tool sources, container commands, and content templates.
- AvalonDock XML import/export, content callbacks, missing content, hidden and
  floating items, malformed input, streams, files, and round trips.
- Headless user input and lifecycle tests; sample ViewModel tests; executable
  sample with upstream license and attribution preserved.
- Release builds and NuGet pack/consumer validation for supported frameworks;
  CI integration and an open PR with incremental commits.

Current status: implementation in progress. The checklist above is open until
supported by corresponding implementation and test evidence.

## Verified implementation

- `DockingManager` template renders `DockControl` using the same live
  `Dock.Model.Avalonia.RootDock` and factory exposed by the compatibility model.
- Panels, document/anchorable panes, typed pane groups, documents, and tools;
  observable collections synchronize with native Dock collection replacement,
  insertion, movement, selection, and close operations. Wrapper identity is
  retained using a per-tree weak identity map.
- Document close cancellation, tool hide/restore, hidden collection, active
  content, and title/content/capability forwarding.
- Anchorable edge placement with `AddToLayout`, writable visibility, cancelable
  hiding callbacks, tab-index restoration, and `Show` insertion strategies.
  Hidden auto-hide tools retain their group and original pane across XML saves.
- Observable document/tool sources, source replacement, layout replacement,
  insertion strategies, and source removal from visible, hidden, and pinned
  locations. Compiled bindings and data templates render ReactiveUI ViewModel
  contents; source subscriptions do not retain unused managers.
- Layout cleanup retains the last document pane and referenced return panes,
  clears detached active content, and collapses redundant typed pane groups.
- Auto-hide sides and groups project Dock's native pinned lists, preserving
  public group ownership through pin, preview, unpin, collection replacement,
  and reordering. Headless pointer input opens the sample's Agenda strip.
- Compiled Avalonia default and Xceed XML namespaces; headless pointer tab selection and text input.
  Pixel/star pane dimensions translate to Dock proportions, with headless
  verification of runtime sizing, vertical sizing, and pointer splitter drag
  persistence across window resizing.
- XML for the docked hierarchy, auto-hide groups and hidden tools, callbacks, content identities,
  selected documents, basic metadata and constraints, file/stream/text APIs,
  malformed input rejection, and atomic manager layout replacement.
- Xceed Live Explorer 3.7.0 sample adaptation with its Ms-PL license, source
  notice, passive views, ReactiveUI commands, and save/load integration tests.
- 68 passing tests in `Dock.Avalonia.AvalonDock.Tests` as of this checkpoint.
  NuGet pack produces net8.0 and net10.0 assemblies, XML docs, and the package
  documentation with dependencies on Dock's Avalonia model and Fluent theme.

## Open work (not release-complete)

- Floating-window models and corresponding XML, native window lifecycle and
  state synchronization. The XML writer explicitly rejects populated floating
  layouts to prevent silent data loss.
- Remaining manager APIs: item containers and commands, additional update
  strategy invocation paths, templates/selectors, styles, themes, routed
  events, converters, and the control adapter surface.
- All upstream interfaces, extension methods, protected extensibility points,
  and `IXmlSerializable` methods not yet represented in the adapter.
- Complete auto sizing, themed splitter sizing, header visibility,
  repositioning policies, icons, tooltips, enablement, and floating dimensions
  in actual rendering and behavior. Several metadata properties currently
  round-trip without applying all upstream visual or behavioral effects.
- Full native drag/float/redock and lifecycle coverage, including native-created
  nested groups, layout detachment and leak checks, and source-driven floating
  content and changes during deserialization.
- Exact signatures, framework substitutions, inheritance, defaults, and member
  behavior must be audited independently of the presence inventory.
- Complete the sample's floating-window persistence paths.

## Reproducible API inventory

`UPSTREAM-API.txt` records 951 public/protected declarations across 102 exported
types in the pinned upstream source. The Roslyn-based source audit counts 260
matching type/member keys and 691 missing declarations at this checkpoint.
This is a **presence audit only**, not a percentage of completed compatibility.
It includes the upstream control, theme, converter, and resource surfaces.

```sh
dotnet run --project build/AvalonDockApiAudit -- snapshot \
  /path/to/wpftoolkit/ExtendedWPFToolkitSolution/Src/Xceed.Wpf.AvalonDock \
  src/Dock.Avalonia.AvalonDock/UPSTREAM-API.txt
dotnet run --project build/AvalonDockApiAudit -- compare \
  src/Dock.Avalonia.AvalonDock/UPSTREAM-API.txt src/Dock.Avalonia.AvalonDock
```

The comparison deliberately returns a nonzero status while declarations are
missing. Comparing the snapshot to its pinned upstream source returns 951/951.
Neither that self-check nor a matching member key verifies signature or
behavioral parity. Do not make the check pass by filtering out missing API.
