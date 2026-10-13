# Review of declaration mismatches

The pinned upstream inventory has 951 declarations. The strict source inventory
finds 850 matching keys. All 101 unmatched keys have now been classified in
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
| Native arrangement | 1 | Native arrangement and current overlay-area projection replace the WPF manager's cached-area invalidation. |

The 100 mapped source declarations in the first four rows were checked against
the adapter's source inventory. Each TSV row retains the original upstream key,
its classification, and the corresponding adapter declaration or implementation.

## Framework and routing differences

Avalonia property-change arguments, routed events, pointer input and focus are
framework substitutions. They do not expose every WPF event-argument field or
dependency-property metadata behavior. Existing Headless fixtures exercise the
hooks and their routes; application overrides must use the Avalonia signatures.

Nine explicit XML write overrides and nine read overrides, including document
and tool content overrides, now have exact presence keys. Typed attribute helpers
follow the inheritance chain; generic base readers and writers handle children
without inferring concrete metadata from additional interfaces. Custom read/write
overrides participate through generated application catalogs or the explicit `XmlLayoutSerializer.KnownTypes` registry.

External classes can derive from `LayoutGroupBase` using its protected parameterless
constructor, matching the implicit constructor in upstream source. Each instance
has a separate native Dock container. The syntax inventory omits implicit constructors;
external compilation and notification/ownership tests verify this contract separately.

`LayoutDocumentPane` and `LayoutAnchorablePaneGroup` now declare their visibility
overrides, recomputing the current parent's visibility before the base call, which
recomputes it again. Reparenting, suppression, and errors preserve that call order.

`LayoutAnchorableItem` now declares its three default-command binding/lifetime
overrides. Tool commands precede common commands; initialization, application,
clearing, callback failures, and reinitialization follow the declared inheritance
order. These corrections have external consumer and Headless coverage.

`LayoutDocumentFloatingWindowControl` now declares its `OnClosed` override.
Both typed floating controls retain constructor-model observation through the
base close lifecycle and unsubscribe only after it returns successfully. The
base collects its captured root before `Closed`, with native factory notification
preserved by `HostWindow.OnClosedCore`.

The protected `LogicalChildren` declarations on `DockingManager`,
`LayoutDocumentPaneControl`, and `LayoutAutoHideWindowControl` now have matching
presence keys. Their virtual enumerators supply the framework's logical source;
manager public enumeration retains weak raw entries, while public framework
traversal filters non-logical values. Internal lifecycle propagation also visits
physically owned children. This removes three former mismatches.

The WPF manager's `ArrangeOverride` invalidates cached overlay areas. The adapter
uses native arrangement and captures current areas through
`LayoutOverlayProjection`; it does not keep that WPF cache. The manager now
declares `OnKeyDown(KeyEventArgs)`: its base call handles Shift+F10 using the
current active content's public visual parent. Opening happens on key press;
canceling or suppressing the base call prevents a second request on key release.
The popup stays open through the opening shortcut's release. This matching
presence key still uses Avalonia's key arguments, not WPF's argument type.
`LayoutContextMenuRouting` continues to handle ordinary framework requests,
including the Apps key and menus owned by editor content. Ctrl+Tab has its
separate `OnPreviewKeyDown` compatibility hook.

`LayoutAutoHideWindowControl.BuildWindowCore(HandleRef)` and
`DestroyWindowCore(HandleRef)` now have matching declaration keys. They execute
real `NativeControlHost` creation and destruction around the existing presenter,
using an embeddable framework root on Windows/macOS. Headless and X11 retain
inline presentation because their backends do not support embedding; hooks are
not fabricated. Their platform behavior remains separate validation evidence.

## What this review does not verify

Matching names and parameter keys do not prove return types, accessibility,
generic constraints, inheritance substitutability, property defaults, event
ordering, styling, rendering, or lifecycle behavior. These require the focused
contract tests and platform validation listed in
[`COMPATIBILITY.md`](../../src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md).

The strict inventory deliberately continues to report 101 unmatched declarations.
Do not subtract these rows from the upstream baseline or report 951 equivalent
APIs on the strength of this classification.
