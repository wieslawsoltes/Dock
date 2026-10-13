# Review of declaration mismatches

The pinned upstream inventory has 951 declarations. The strict source inventory
finds 838 matching keys. All 113 unmatched keys have now been classified in
[`portability-review.tsv`](portability-review.tsv); the original inventory and
comparison rules remain unchanged.

This review explains the mismatches. It does not establish behavioral parity,
binary compatibility, or completion of the compatibility project.

| Mismatch | Count | Adapter contract |
| --- | ---: | --- |
| Property callback argument | 61 | The same hook name accepts `AvaloniaPropertyChangedEventArgs`. |
| Mouse, keyboard, or focus argument | 32 | The same hook name accepts Avalonia pointer, key, or focus arguments. Press and release use their distinct event types. |
| Context-menu opening argument | 1 | `ContextMenuEx.OnOpened` accepts Avalonia's `RoutedEventArgs`. |
| Visual/logical tree extension receiver | 6 | `Extentions` accepts `AvaloniaObject`; traversal follows Avalonia trees. |
| Member inherited from an adapter base | 6 | Three command/binding hooks, two visibility hooks, and document-window closure remain callable or overridable through their base classes. |
| Framework logical-child contract | 3 | Avalonia supplies a logical-child collection, rather than WPF's overridable enumerator property. |
| Native layout/input routing | 2 | Native arrangement and context-menu request routing replace manager-specific WPF overrides. |
| WPF `HwndHost` hooks | 2 | No equivalent hook is supplied by the native pinned-content presenter. |

The 106 mapped source declarations in the first five rows were checked against
the adapter's source inventory. Each TSV row retains the original upstream key,
its classification, and the corresponding adapter declaration or implementation.
An inherited declaration points to its declaring base type; it is not counted as
a declaration on the derived type by `compare`.

## Framework and routing differences

Avalonia property-change arguments, routed events, pointer input and focus are
framework substitutions. They do not expose every WPF event-argument field or
dependency-property metadata behavior. Existing Headless fixtures exercise the
hooks and their routes; application overrides must use the Avalonia signatures.

Nine explicit XML write overrides and nine read overrides, including document
and tool content overrides, now have exact presence keys. Typed attribute helpers
follow the inheritance chain; generic base readers and writers handle children
without inferring concrete metadata from additional interfaces. Custom read/write
overrides participate through the explicit `XmlLayoutSerializer.KnownTypes` registry.

External classes can derive from `LayoutGroupBase` using its protected parameterless
constructor, matching the implicit constructor in upstream source. Each instance
has a separate native Dock container. The syntax inventory omits implicit constructors;
external compilation and notification/ownership tests verify this contract separately.

The three `LogicalChildren` mismatches are `DockingManager`,
`LayoutDocumentPaneControl`, and `LayoutAutoHideWindowControl`. Their protected
collection and `ILogical.LogicalChildren` follow Avalonia's ownership rules.
`DockingManager.LogicalChildrenPublic` exposes an enumerator for callers. This
does not reproduce WPF's logical-tree shape or its virtual enumerator override.

The WPF manager's `ArrangeOverride` invalidates cached overlay areas. The adapter
uses native arrangement and captures current areas through
`LayoutOverlayProjection`; it does not keep that WPF cache. The manager's WPF
`OnKeyDown` handles Shift+F10. The adapter routes framework context-menu requests
through `LayoutContextMenuRouting`, including keyboard requests and menus owned
by editor content. An inherited `OnKeyDown` override is available, but the menu
route is not implemented inside that override. Ctrl+Tab has its separate
`OnPreviewKeyDown` compatibility hook.

`LayoutAutoHideWindowControl.BuildWindowCore(HandleRef)` and
`DestroyWindowCore(HandleRef)` belong to WPF's `HwndHost`. The adapter composes
content inside Dock's native pinned host. It does not create a replacement HWND
or provide dummy implementations of these hooks.

## What this review does not verify

Matching names and parameter keys do not prove return types, accessibility,
generic constraints, inheritance substitutability, property defaults, event
ordering, styling, rendering, or lifecycle behavior. These require the focused
contract tests and platform validation listed in
[`COMPATIBILITY.md`](../../src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md).

The strict inventory deliberately continues to report 113 unmatched declarations.
Do not subtract these rows from the upstream baseline or report 951 equivalent
APIs on the strength of this classification.
