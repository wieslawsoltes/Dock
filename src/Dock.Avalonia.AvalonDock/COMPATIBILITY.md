# Compatibility work and acceptance criteria

The objective remains complete AvalonDock C# and XAML API compatibility on
Avalonia, using Dock's Avalonia models and docking implementation, a NuGet
package, and a licensed adaptation of the Xceed sample. This document records
implementation evidence and portability boundaries; a passing subset of tests is
not proof of exact WPF compatibility.

Reference: Xceed `wpftoolkit` commit
`2c71faba5eecc1b6ae6cd3d269408e0df37715d8`, including public layout types,
interfaces, extension methods, `DockingManager`, controls, commands, themes,
converters, events, and XML serialization. Public WPF framework types require
Avalonia counterparts; binary compatibility with WPF is not possible.

The public CLR namespaces intentionally use `AvalonDock.*`, dropping the
upstream `Xceed.Wpf` prefix. Source consumers update their using directives;
the API inventory applies this explicit namespace mapping. The default
Avalonia XML namespace and the Xceed XML namespace alias are both supported.

Acceptance requirements:

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

The latest automated contract pass contains **3,876 adapter cases and 30 rendering
cases**, with no skipped cases. It includes 30 added auto-hide focus/resize cases,
alongside the earlier manager key-down and inherited-control-property cases described below. The matrix makes the acceptance evidence explicit; it
replaces the former open-ended lists of manager, interface, callback, and default
work. Each contract remains subject to the framework boundaries and deferred
interactive checks at the end of this document. This is not a claim of exact WPF
behavior or binary compatibility.

## Acceptance evidence

The upstream reference is the pinned source above. The strict declaration audit
retains all 951 entries: 845 keys match and 106 have individual classifications.
All 102 exported type keys are present. The signature review separately covers
parameter names, modifiers, generic constraints, differing base lists, and the
implicit constructor omitted by the syntax inventory. No unmatched declaration
is silently removed from the baseline.

| Requirement | Implementation and representative verification |
| --- | --- |
| Public models and interfaces | `LayoutInterfaceMembershipTests`, `LayoutGroupContractTests`, `LayoutExtensionTests`, and `GroupBaseDerivationTests` exercise concrete interface membership, application-defined containers, traversal, external subclass construction, and group notifications. The external `LayoutInterfaceProbe` and `GroupBaseDerivationProbe` compile against NuGet only. |
| Property defaults and inherited behavior | The final source review compares 84 styled registration defaults, reviews 26 direct registrations, and checks the 27 non-style-key metadata overrides. `ManagerPropertyTests`, `LayoutMetadataContractTests`, `ContentPropertyBindingContractTests`, and `InheritedControlPropertyTests` cover priority, clearing, binding lifetime, model-backed values, and custom themes. |
| Compiled XAML and namespaces | `CompiledXamlTests` and the compiled layout, source, template, sizing, menu, theme, and floating fixtures use both the default Avalonia namespace and historical Xceed alias. The package consumer repeats these compilation checks without project references. Production and sample bindings use the compiled path. |
| Live native identity and ownership | `LayoutBridgeTests`, `LayoutParentPolicyTests`, `ParentTransitionContractTests`, `RootSideLifetimeContractTests`, `CollectionReplacementContractTests`, and `CollectionReentrancyContractTests` cover both wrapper and native edits, callback ordering, ownership, failures, and retention. |
| Selection, activation, and model operations | `PaneSelectionContractTests`, `ContentActivationHookReferenceTests`, `ModelDockingContractTests`, `RootUpdatedContractTests`, and `GarbageCollectionContractTests` cover independent selection/activation, history, explicit operations, root callbacks, and return locations. Reference probes retain recorded WPF traces and separately record Avalonia differences. |
| Docking, floating, hiding, and capabilities | Native policies and factory operations implement the movements. `NativeFloatingGroupTests`, `FloatingDropPolicyTests`, `OuterSplitTests`, `AutoHideOperationTests`, and `DuplicateContentDropTests` cover typed and mixed groups, capabilities, cancellations, restoration, and target visibility/execution. |
| Manager, sources, templates, and commands | The template, layout, source, resource/timer, container-style, and root-registration audits below cover callback arguments, suppression, replay, reentrancy, failure phases, and weak subscriptions. `DefaultCommandActionContractTests`, `ItemMetadataCallbackTests`, and `ContentControlCallbackTests` cover container and command behavior. |
| Manager keyboard menus | `ManagerKeyDownContractTests` and the external `ManagerKeyDownProbe` cover the protected base hook, active visual/model lookup, custom controls without an item, origin placement, override replacement/suppression, cancellation, and key press/release routing with Windows-style shortcuts. Editor-owned menus and the Apps key retain Avalonia routes. |
| Grids, splitters, tabs, anchors, and overlays | `GridControlTests`, `SplitterSizingTests`, `AutoSizingTests`, `TabControlPolicyTests`, `PaneInputContractTests`, `AutoHideResizeTests`, and `OverlayWindowTests` exercise public controls with native layout, input, live policy changes, and replacement themes. |
| Full-content drag previews | `FullDragPreviewTests`, the Skia `FullDragPreviewImageTests`, and the package probe cover document/tool/pane snapshots, content size and display density, per-manager settings, cancellation, and disposal. Exact projected destination layouts are a separate native option, as explained below. |
| Floating lifecycle, focus, and navigator | The floating close/override/closed-hook/focus tests and navigator owner/focus tests cover Headless lifecycle and input contracts. Retained editor/model/window tests cover detachment and weak lifetimes. These do not replace the explicitly deferred desktop checks. |
| XML persistence | Read/write inheritance, numeric, root, group, content, streaming, writer-context, malformed-input, recovery, reentrancy, source, and floating serialization tests cover all overloads and registered custom types. Package consumers repeat public serialization hooks and file/stream round trips. |
| Themes, resources, and converters | Theme callback/lifetime tests, localization tests, tree helpers, converter tests, and the image suite cover compiled dictionaries, window propagation, culture fallback, all 13 satellites, public utilities, and URI images. Custom resource/type registration stays reflection-free. |
| Sample and documentation | `DockAvalonDockSample` supplies passive views, ReactiveUI commands, a workspace service, compiled bindings, and the retained sample license/attribution. `SampleTests` covers its ViewModel behavior. The guide and PR include usage examples and architecture diagrams; the DocFX pipeline includes adapter API metadata. |
| Release integration | The package-only consumer targets .NET 8 and .NET 10. Validation packs all 11 dependency/adapter packages and symbols, checks repository commits and assembly/documentation/satellite hashes, and executes the consumer on both runtimes. CI builds/tests Windows, Linux, and macOS, runs native leak/AOT checks, and validates packing. The PR records the validated commit and results. |

The type/reference review also checked exported APIs used through inference or
extension syntax: layout and visual-tree extension classes, event argument types,
`OverlayArea`, `ILayoutDocumentPane`, and `LayoutGroup<T>`. Their behavior is covered
through concrete types, generic constraints, event traces, and extension calls;
a text search for direct type names is not a coverage measure.

The 84 registration-default comparisons have no unexplained value mismatch.
Direct `Title` and `ContentId` properties use the reflection-free framework
value policy for WPF binding replacement, current values, and clearing. The inherited metadata review exposed
and fixed theme-dependent floating/overlay/splitter defaults and constructor-local
auto-hide focus/visibility values. WPF style keys map to Avalonia control themes;
WPF transparency and decorations map to Avalonia's corresponding window properties.

## Native Dock prerequisites

The shared Dock changes are maintained separately from the AvalonDock facade.
[PR #1160](https://github.com/wieslawsoltes/Dock/pull/1160) merged the native policies,
host lifecycle support, sizing hooks, and interaction fixes into the repository's
default branch, `master`. This branch includes that merge.
The native projects have no reference to the AvalonDock assembly or namespaces.

| Compatibility requirement | Native Dock support | Work owned by the adapter |
| --- | --- | --- |
| Drop restrictions and tool-to-document splits | `IDockDropPolicy`, `IDockSplitPolicy`, native validation and pane creation | Manager identity, floating destination rules, and public pane types |
| Split beside an enclosing document region | Outer operations and `IDockOuterSplitPolicy`; ordered movement of whole panes/groups | Resolve the public document region and normalize its public hierarchy |
| Preview/complete operation notifications | `IDockOperationObserver` and disposable operation scopes | Routed AvalonDock events and nested-operation coalescing |
| Independent tab dragging and reordering | `IDockItemDragPolicy` in native tab/pin input | `CanMove`, `IsEnabled`, and `CanRepositionItems` mapping |
| Selection distinct from activation | Factory focus, selected-flag synchronization, and replacement-selection hooks; Avalonia-model `OwnerChanging` | Public selection notifications, activation history, and parent/root projection |
| Explicit model close and auto-hide operations | Protected close/pin core operations retaining native cancellation and cleanup | AvalonDock command-versus-model capability semantics and return history |
| Model-aware floating hosts and canceled closure | `IDockWindowHostFactory`, `IHostWindowCloseState`, and `IDockWindowClosePolicy` | Typed public hosts and public manager registration |
| Close a host while retaining its layout | Desktop and managed host policy implementations; managed reentrancy and replacement-host handling | Deferred WPF ordering and public collection semantics |
| Collect empty layouts before `Closed` | `HostWindow.OnClosedCore(EventArgs, IDockWindow?)` after capturing the original model/factory and closed state ([PR #1176](https://github.com/wieslawsoltes/Dock/pull/1176)) | Captured-root collection, typed subscriptions, binding cleanup, and callback failure order |
| Custom docking overlays | Scoped window factories/themes, enclosing preview hosts, selector registrations, `IDockTargetPresentation` and geometry | AvalonDock named window parts, preview path, selector mapping, and public snapshots |
| Pixel/auto/star sizing and resize previews | Custom splitter host, native tracker/input, preview properties, pinned sizing and custom thumb support | Public grid lengths, normalization, and size notifications |
| Auto-hide resize limits and unset sizes | `PinnedDockControl.MinimumRemainingContentSize` and `InitializePinnedBoundsOnLayout` ([PR #1183](https://github.com/wieslawsoltes/Dock/pull/1183)); defaults remain 50 and true | Scoped zero/false settings, initial fallback dimensions, physical ghost bounds, and commit-time model coercion |
| Stable presentation and cleanup | Deferred root presentation, create-button coexistence, hidden-window filtering and tool-menu lifetime fixes | Public templates, layout items, themes, and retained-model subscriptions |

The [native extension guide](https://github.com/wieslawsoltes/Dock/blob/master/docfx/articles/dock-extension-policies.md)
documents contract defaults, usage, architecture, and regression coverage.
Validation is performed in a native-only worktree before reintegration, including
all model implementations and Avalonia 11 linked controls.

Framework-specific event arguments, logical trees, Win32 hooks, general
resource-URI loading, custom XML type reconstruction, and public compatibility callbacks belong to the
Avalonia/framework or facade boundary. A declaration-only mismatch is not evidence
of a missing native docking feature. Deferred floating cleanup uses the existing
virtual factory and host contracts; its public registration timing belongs to the
facade. Any additional native capability must follow the same separate-PR,
merge-to-`master`, then reintegrate workflow.

The floating-model lifetime audit found another native issue: a retained Avalonia
layout remained a logical child of its closed `HostWindow`. The isolated fix in
[PR #1161](https://github.com/wieslawsoltes/Dock/pull/1161) detaches model content
before `Closed` callbacks while preserving content values, cancellation, and
layout-preserving close policies. Its seven new native Headless cases cover model
retention, successful/canceled closure, and application control content; existing
replacement-host cases also verify the model can attach to its new host.

Nested layouts also exposed retained presenters, including a tool template that
had already left the visual tree. The separate native fix in
[PR #1162](https://github.com/wieslawsoltes/Dock/pull/1162) releases model bindings
and deferred visual content before `Closed`, preserving the model graph for a
replacement host. Its 16 new Headless cases cover documents/tools, local themes,
cancellation, replacement hosts, and explicit deferred-presentation cleanup.

[PR #1165](https://github.com/wieslawsoltes/Dock/pull/1165) resolves Avalonia 12's
presentation wrapper to its owning top level. Native pinned windows now receive
an actual owner, and activation, coordinate conversion, and other native owner
lookups use the same corrected helper. All 1,748 affected native UI, unit, and
leak cases and all nine GitHub checks passed. This branch includes that master
merge and the 12.1.0.8 version update.

[PR #1166](https://github.com/wieslawsoltes/Dock/pull/1166) keeps a native pinned
preview open while activation moves between it and its owner, and closes the
preview through Dock when both lose activation. It also preserves floating
window title/icon values and bindings during default owner lookup. All 1,752
affected native Headless, unit, and leak cases passed before this master merge.
The adapter adds native preview content/theme/input bindings and matches WPF's
owned-window shutdown contract. Combined desktop and package validation follows
this integration.

[PR #1167](https://github.com/wieslawsoltes/Dock/pull/1167) adds optional external
target presentations. Its 18 new cases exercise all nine operations, transformed
cross-window selectors, visibility/validation vetoes, indicator-only mode,
direction and edge restrictions, reentrant replacement, and detached selectors.
All 1,770 affected native cases and all nine GitHub checks passed before merging
`master` into this branch. An existing Windows create-button lifetime case failed
once; the failed CI job passed unchanged on retry. The native code continues to
own hit testing and docking policy; the adapter supplies only the presentation.

[PR #1168](https://github.com/wieslawsoltes/Dock/pull/1168) adds
`IProportionalSplitterHost` and `DockableControlTracker`. Public grids use these
native services while exposing direct pane/splitter children and real Grid
definitions. Local validation passed 105 unit, 1,552 Headless and 128 leak cases;
the final registration-ownership guard passed seven tracker cases, including
three new replacement cases. All nine GitHub checks passed, including the three
platforms, leak tests, AOT and packaging. The PR is merged through `master`.

[PR #1169](https://github.com/wieslawsoltes/Dock/pull/1169) corrects tracked
coordinates for controls with a nonzero parent offset and initializes bounds
when a presentation changes models without another layout pass. The native
tracker uses the control's local origin; 13 focused cases passed before the
separate PR was merged through `master` and reintegrated here.
All nine GitHub checks also passed for this PR.

The direct-grid integration scopes its native body templates to prevent a second
public pane from being generated inside each pane. Model minimums belong to the
grid definitions, leaving public pane themes free to set their own control
minimums. Compatibility grid styles disable layout rounding, matching WPF's
default and preserving exact pixel resize limits when fixed panes leave spare
space. An empty middle pane leaves one splitter between the visible siblings,
as in the upstream grid. The consolidated adapter suite passes all 2,422 cases,
including direct-grid, overlay, retained-content, reattachment, return-container,
metadata notification, and content-comparison cases.

The lifetime investigation identified a retained editor's cached Avalonia
composition visuals as a path to a closed pinned window after the manager had
already been released. Item presenters now release their composition children
on detachment, and rebuild them on reattachment. Overlay presenters do the same
for cached native targets and replaced templates. This uses public composition
APIs without reflection and preserves the application editor and its bindings.
Overlay geometry retains fractional local coordinates across window origins,
DPI scales and visual transforms; screen-coordinate snapshots remain physical
pixels. Package and consumer validation passed at `e04288110`; the final desktop
checks are deferred at the user's request.

[PR #1174](https://github.com/wieslawsoltes/Dock/pull/1174) repairs the shared API
documentation pipeline. DocFX reads each primary library's compiled metadata,
XML comments, and Source Link PDB, including generated ReactiveUI members.
Generated sources no longer pollute shared project directories, and failed
stages return a failing script exit. The native full build and its master CI
deployment pass; after integration, the complete AvalonDock API and article
build also passes with zero DocFX warnings or errors. This branch includes
the master merge at `0141ac063`.

### Recent native drag and preview changes

The earlier `master` integration at `805fdb68a` included
[PR #1153](https://github.com/wieslawsoltes/Dock/pull/1153) and
[PR #1156](https://github.com/wieslawsoltes/Dock/pull/1156). Their relevance to the
adapter is:

| Native change | Effect on AvalonDock compatibility |
| --- | --- |
| Reusable regular adorners and interrupted window-drag cleanup | Existing native hosts and overlays detach old parents, clear pending targets on cancellation/closure, and avoid duplicate drag-end notifications. |
| Selector coordinate and enclosing-region fixes | Existing public overlay snapshots and outer-document selectors use the corrected native hit testing, including separately hosted and scaled controls. |
| Escape cancellation and tab-source previews | Native input cancels across focus/capture changes. Dragging a tab can temporarily select another tab; cancellation restores the prior selection unless the application changed it. Public selection notifications still apply. |
| Global split distribution | Native docking rescales retained siblings and sizes the resulting pane, including whole-pane moves. |
| Scrollable-source and mixed-display preview sizing | The adapter's native drag preview benefits from finite measurement and macOS logical-coordinate handling across Retina boundaries. |
| Full-content drag previews ([PR #1170](https://github.com/wieslawsoltes/Dock/pull/1170), [PR #1171](https://github.com/wieslawsoltes/Dock/pull/1171), [PR #1173](https://github.com/wieslawsoltes/Dock/pull/1173)) | The manager enables full content by default through `IDockDragPreviewPolicy`; native and managed hosts capture registered visuals before tab selection changes, and document previews use content dimensions. Snapshots retain their logical size and full pixel content at high display scaling. `ShowDockablePreviewOnDrag` can select compact previews per manager. |
| Isolated projected layouts | `IDockPreviewFactoryProvider` and `DockSplitPreview` provide a future integration point; exact projections are not enabled for the compatibility layout. |

Exact projection has two independent limitations here. `LayoutFactory` derives
from the Avalonia factory and deliberately retains its default null preview
factory: a stock clone would lose the adapter's typed document splits, outer
region policy, mixed-orientation rules, and removal selection. In addition,
the public grids use Avalonia Grid definitions for AvalonDock lengths;
native projection conservatively falls back for custom splitter hosts. The custom local
target template currently uses its existing half-pane and third-region indicators.
Enabling exact projection requires isolated policies, a matching sizing contract,
and corresponding template/geometry tests together. Simply returning a stock
factory would not establish compatibility.

The exact-projection limitation concerns predicting the destination pane's
bounds. It does not disable full-content previews that follow the pointer.

The incoming Avalonia 11 preview suite validates native shared controls. It does
not add an Avalonia 11 target to this adapter. The source-generator changes select
the appropriate System.Text.Json generator in shared compiler processes; the
AvalonDock XML codec remains separate. These native changes do not determine manager callback contracts or platform
behavior. Custom CLR type reconstruction uses the explicit registry documented
below rather than automatic discovery.


## Verified implementation

- Floating hosts remove their manager registration and collect the captured root
  before `Closed`, without collecting again after its callbacks. The native
  `HostWindow.OnClosedCore` hook from
  [PR #1176](https://github.com/wieslawsoltes/Dock/pull/1176) preserves the original
  factory notification when that collection removes an empty floating model.
  Typed controls retain constructor-model observation through the base lifecycle;
  successful return then releases observation and tool bindings/owned commands.
  Callback errors stop the following stages. Twenty Headless cases cover phase
  order, redirected models, suppression, failure/replay, and binding lifetime.
- Tool containers declare their default binding/lifetime overrides and process
  hide, auto-hide, and dock before common item commands. Derived initialization
  is safe before attachment and does not evaluate command predicates. Application
  and clearing read live definitions after each callback, preserve application
  commands, and stop on failures. Reinitialization and definition clearing retain
  assigned commands; native detachment disables and releases owned commands.
  Default `CanExecute` queries read current state while native/root notifications
  continue to raise `CanExecuteChanged`. Local binding expressions to current
  defaults are cleared; raw observable subscriptions retain Avalonia ownership
  (use compiled bindings or `ToBinding()` for expression cleanup).
- The consolidated adapter suite passes all 3,501 cases, including 26 root-update cases, 39 parent/command/focus cases, 23 side-lifetime and 37 root-property cases and root replacement cancellation, 36 group-minimum and 52 model-docking cases, 100 focus/direct-command cases, 20 floating-close lifecycle cases and 27 default-command initialization, binding, callback,
  reentrancy and inheritance cases, plus 31 root XML, 14 interface-membership/dispatch, and eight visibility inheritance cases, plus 35 XML writer-context, error-boundary, flush, lifetime and encoding cases, plus 17 XML read-inheritance and external base-group cases, plus 47 XML write-attribute, inheritance, identifier,
  callback and failure cases, plus 43 streaming XML
  reader, partial-state, floating replacement, geometry, custom-hook and error-boundary
  cases, plus 63 numeric XML
  formatting, culture, round-trip, and enum-error cases, plus 10 tool-pane XML
  selection, callback, failure, recovery, and nested-read cases, plus 51 content XML
  metadata, setter-order, selection, focus, floating-state, reader-position, failure,
  custom child-element, and thread-access cases, plus 58 group XML metadata, notification, failure, visibility, enum, and pane-name cases, plus 39 layout
  helper/group contracts and the recorded activation and membership ordering
  checks, plus 51 template
  callback delivery, reentrancy, failure, and replay cases, plus 19 layout
  callback argument, ownership, presentation, and lifetime cases, plus 17 root-event
  registration, reentrancy, attachment-order, and lifetime cases, plus 22 container
  style callback, command-order, and native restyling cases, plus 29 source
  callback, registration, layout-transition, and lifetime cases. All 30 rendered
  preview/image cases pass. The source callback audit below records argument
  handling and weak delivery semantics. Coverage also includes 30 theme
  and auto-hide timer callback cases; 24 reproduced failures before correction.
  The focused callback run passed 127 cases before the final initialization
  case was added. The 16 manager property/style/default/hook cases remain
  covered; the first 13 reproduced failures before their correction. The manager audit below records conditional styles, compiled
  bindings, layout coercion, and protected callbacks. The 29 floating-control
  override, command-subscription, closing-order, null-model, and writable-property
  cases remain covered; 27 reproduced failures before their correction. Dropdown input,
  command ordering, nested/child menus, resource updates, and menu lifetime
  pass all 41 focused cases; 14 cases failed before correction. All 30
  rendered-preview/image cases also pass. Tab/title activation, middle-click
  commands, explicit native policy precedence, and title dragging pass 325
  focused cases. Nineteen input/command cases failed before their correction.
  Title drags now distinguish a visual tool-pane ancestor from a standalone
  title, including titles hosted under another manager. The earlier pane/content
  input correction passed its 97 focused cases and remains covered by the full
  suite. Desktop checks remain deferred at the user's request.
- Eight deserialization reentrancy cases cover actual nested XML restores using
  the same or separate serializers, inner callback failures, replacing both
  sources during recovery, and restoration from `LayoutChanged`. Source
  subscriptions resume after failures, retired roots release their manager, and
  committed callback side effects remain observable. The focused serialization
  suite passes all 97 cases, and the consolidated adapter suite passes all 2,430.
- Native document split targets create `LayoutDocumentPane` for documents and
  tools, within a `LayoutDocumentPaneGroup`. Existing groups retain their axis
  or nest an orthogonal split when mixed orientation is enabled. Tool-pane drops
  preserve content/container identity and source tab order, including groups of
  three or more tools. `IDockSplitPolicy` lets the compatibility factory select a
  document destination while Dock owns pane creation and movement; ordinary
  factories retain their tool-pane default. Headless tests cover four edges,
  docked/floating content, nested groups, validation without mutation and XML
  restoration. Native unit tests and the package consumer cover the policy and
  multi-tab order.
- `DocumentPaneDockAsAnchorable*` selectors move tools beside the highest document
  region even with `CanDockAsTabbedDocument=false`. The native outer-split policy
  preserves dragged panes/groups, tab order, containers and active content.
  Non-mixed groups restrict selectors to their outside edges. Native capabilities,
  docking groups and directional masks apply; validation leaves the layout intact.
  Whole-group drops remove empty floating return panes, and XML writes public
  return history without reviving a cleared location from native history.
  Scoped native target themes work with reused overlays; weak part registrations
  survive namescope collection without retaining retired visuals.

- Document-list entries use public `MenuItemEx` containers while retaining native
  flyout input and activation. Icon selectors receive the current icon and its
  container; null results preserve the icon, and generated templates inherit the
  public layout model from the entry. The manager theme supplies the default icon
  template. Retiring an entry releases its generated menu presentation and model
  references. A null menu-header assignment inherits the document header even
  with a menu selector, and a non-null inherited template is rendered first.
  Docked/floating tests cover live template changes, selector arguments, reopen,
  default icon metadata, and retained-presentation lifetime.
- `GetLayoutItemFromModel` is a passive lookup, including a null input. Ordinary
  layout attachment registers containers eagerly; source batches defer each
  container and its style until its after callback finishes. Styles see completed
  model metadata, and callbacks can pump the dispatcher or create floating hosts
  without prematurely creating the container. Native content/tab/title controls
  and command chrome observe eventual registration through weak subscriptions.
  Initial handled imports and live ownership checks follow their distinct
  upstream rules. Removed containers survive until deferred cleanup, preserving
  native moves. Source scope cleanup also runs when a callback throws.
- Update strategies are read separately before and after source insertion,
  fallback document/tool docking, and hidden-tool restoration. Callbacks may
  replace or clear the strategy, including during a multi-item batch. Initial
  imports retain their supplied root while live callbacks read the manager's
  current root after layout replacement. `Show()` honors callback changes to the
  return pane/index without briefly reinserting into the old pane; clearing the
  destination leaves the tool hidden. Docking the last-focused document itself
  falls back to the first document pane. Headless regressions cover all seven
  insertion paths, handled/default placement, native ownership, and callback order.
- `UriSourceToBitmapImageConverter` loads HTTP/HTTPS images asynchronously,
  follows redirects, and publishes decoded bitmaps on the UI thread. It supports
  relative URI resolution, an application-owned HTTP client, request/body
  timeouts, disposal, and cancellation on view detachment or source replacement.
  Reattachment retries incomplete loads; completed bitmaps retain their identity.
  Pending requests do not retain unused images. Failures leave an empty image
  and log through Avalonia. A separate Headless xUnit project uses the real Skia
  decoder to validate HTTP and HTTPS loopback responses, redirects, untrusted
  certificates, invalid image bytes, cancellation, and lifetime behavior. WPF
  bitmap download events are represented by Avalonia source updates and logging.
- `DockingManager.OnReceiveWeakEvent(Type, object, EventArgs)` receives live
  source notifications through Avalonia's weak collection event. Overrides can
  observe, handle, or replace a source before default processing. Shared sources
  deliver per attachment with document precedence, and replacing either property
  removes one registration while preserving the other. The portable manager type is
  `WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>`;
  `IWeakEventSubscriber<NotifyCollectionChangedEventArgs>` replaces WPF's listener
  interface. An unhandled subscribed event raises `InvalidOperationException`.
  Initial imports snapshot values before strategy callbacks, skip existing
  layout values/nulls, and retain source duplicates. Initial/range batches keep
  their destination despite callback focus changes and reuse a new fallback
  tool pane. Headless regressions and the package consumer cover these contracts.
- Positionable groups expose `OnDockWidthChanged` and `OnDockHeightChanged`
  after changed-value property notifications. Public assignments, native
  splitter commits, and external native proportion changes invoke these hooks;
  internal measurement updates and XML length restoration do not. Tool-group
  base hooks propagate pixel lengths through single-child tool groups, while
  preserving other units and independent multi-child lengths. Tests cover both
  axes, notification ordering, reentrancy, native previews, and silent XML
  restoration without overwriting existing child dimensions.
- `AvalonDock.Controls.Extentions` exposes all six upstream tree helpers using
  Avalonia objects and their current visual/logical trees. Tests cover ordered
  descendants, type and interface matching, logical-to-visual parent fallback,
  nonvisual logical nodes, null/error contracts, deferred enumeration,
  reparenting, native previews, and layout replacement. The class name retains
  the upstream spelling. Framework argument substitutions remain distinct in
  the API presence audit.
- `AvalonDock.Layout.Extensions` is covered by custom-interface and native-model
  tests for deferred traversal, closest-parent/root lookup, null/value-type
  contracts, type helpers, and nearest-panel side lookup. `GetRoot` walks the
  parent contract rather than reading a custom `Root` property. Layout models
  retain sealed identity equality in both implementations: upstream inherits
  WPF `DependencyObject`, and the adapter inherits `AvaloniaObject`.
- The public group-operation audit verifies invalid replacement indices,
  incompatible child casts, absent/null children, interface lookup, and native
  synchronization. Public removals detach ownership before selection/group
  callbacks. Group and ancestor tree callbacks precede `ChildrenCount`, including
  failure short-circuiting; native auto-hide projections retain Dock's ownership
  transitions. `LayoutExtensionTests` and `LayoutGroupContractTests` add 39 cases,
  supplemented by the existing parent, selection, auto-hide and lifecycle tests
  and an external package consumer. Duplicate/cyclic membership and stale parents
  after `Clear()` are not reproduced: the adapter preserves a valid Dock graph.
- `LayoutAutoHideWindowControl` presents the existing native preview and exposes
  its current public model, background, and anchorable content theme. The
  manager's readonly `AutoHideWindow` and protected hooks expose the generated
  presentation. `AutoHideWindowClosingTimer` supports the upstream default,
  negative-value coercion, idle monitoring, and deadline updates. Active tools,
  pointer interaction, native resizing, and Dock's keep-visible policy hold the
  preview open. Native Dock retains preview ownership, sizing, and content views.
  The native Fluent layout host uses a deferred content presenter, preventing
  Avalonia layout models from becoming template children that retain a replaced
  manager. Deferred realization and retained-layout lifetime are covered.
- Public `LayoutAnchorSideControl`, `LayoutAnchorGroupControl`, and
  `LayoutAnchorControl` expose the auto-hide hierarchy over native pinned tools.
  Manager side-panel properties and protected changed hooks control the actual
  presentation; three anchor templates update generated controls at runtime.
  Constructors remain internal. Default strips retain native pinned input,
  while unhandled presses, 400-millisecond hover, and programmatic selection or
  activation open Dock's preview with the existing content view. Passive preview
  preserves the active document and resets the public selection flag. Tests
  cover both XML namespaces, all edges, collection changes, template replacement,
  pin/unpin, side replacement, XML, detachment, and retained-model lifetime.
- Public `LayoutGridControl<T>`, `LayoutPanelControl`,
  `LayoutDocumentPaneGroupControl`, and `LayoutAnchorablePaneGroupControl` expose
  live model/orientation state over Dock's existing proportional models, with
  direct public pane/splitter children and real Grid definitions.
  Constructors remain internal; styles compile through both XML namespaces.
  `DockingManager.LayoutRootPanel`, its Avalonia property field, and changed hook
  expose/control the actual displayed root, including clear/restore, root/XML
  replacement, and reattachment. Pane groups normalize non-star lengths along
  their split axis on initial layout and structural/visibility/orientation
  rebuilds; plain panels apply no conversion. Grid measurement, native tracking and splitter input,
  pane selection, and existing content views remain in Dock. Tests cover live
  length changes, initial weights, hidden children, floating tool groups, and
  subscription lifetime with a retained old model and manager.
- `DockingManager` template renders `DockControl` using the same live
  `Dock.Model.Avalonia.RootDock` and factory exposed by the compatibility model.
- Panels, document/anchorable panes, typed pane groups, documents, and tools;
  observable collections synchronize with native Dock collection replacement,
  insertion, movement, selection, and close operations. Wrapper identity is
  retained using a per-tree weak identity map.
- Parent/root lifecycle hooks and `LayoutRoot.ElementAdded`/`ElementRemoved`
  work through public and native ownership changes. The native Avalonia model's
  `OwnerChanging` event runs before ownership changes and after thread-access
  validation. Existing wrappers survive native moves between separate trees;
  weak identity metadata does not retain an unrelated rejected destination.
  Hooks expose the public floating-document parent, and existing presenters
  transfer between native hosts during temporary detachment. Tests cover
  ordering, subtree notifications, overrides, rejected assignments, tool
  hide/auto-hide/float cycles, native factory moves, and package consumers.
- Root panel, side, and manager property assignments publish changing and changed
  phases. A null panel creates the default document layout; nullable sides update
  native pinned projections and live manager side controls. Compiled XAML accepts
  null root slots in both namespaces, and empty side XML sections round-trip.
  Construction order, lazy child enumeration order, fixed-slot child counts,
  remove/add replacement notifications, cancellation, and observer failures have
  focused Headless and external package coverage. The native bridge still detaches
  replaced side parents; upstream's retained-parent behavior remains open below.
- Parent-change selection and visibility policies: content deselects while its
  previous parent is still available; initially selected content selects its
  destination pane. Pane and content selection synchronize before a factory is
  attached. Public collection notifications follow ownership initialization,
  and invalid-thread collection changes leave the hierarchy untouched.
  Cached group visibility has a protected setter and `OnIsVisibleChanged` hook,
  propagates through ancestors, and observes document/tool/auto-hide group rules.
  Native-created nested subtrees compute visibility when their wrappers are
  initialized. Tool visibility notifications precede parent notifications, even
  when visibility stays true; auto-hide sides resolve their edge before those
  notifications. Floating-pane detection counts visible panes in the public
  tree, and parent collection subscriptions are released on reparenting.
- Pane selection is independent of activation for both `SelectedContentIndex`
  and content `IsSelected`. Content flags precede parent selection; pane transitions
  publish nested clear notifications before completing their requested index.
  Reentrant observers and exceptions retain the source-defined intermediate state.
  Removing selected rooted documents uses
  activation history with stable child-order ties, including disabled content.
  Detached document panes and tool panes choose the first enabled child. Emptying
  a rooted document pane clears root activation, even with another active tool.
  Child moves run collection policy before protected `ChildMoved` overrides, whose
  index-based update preserves document/tool differences; identical indices are silent. Tool collection changes
  notify aggregate capabilities, which are true for empty panes. Native factory
  selection/focus policies retain ordinary Dock defaults. Native collection
  repair waits until item generators finish; the queued fallback uses weak
  ownership. Native presentation coalesces nested clears without changing public
  callback delivery. Tests cover public/native removals, derived callbacks, reentrancy,
  native floating rendering, and collection of a removed pane with repair queued.
- Protected content selection/activation and closing/closed hooks run through
  public and native operations. State hooks receive old/new values before the
  property-changed notification; activation hooks observe completed pane/root
  state. Base hooks raise their public events and update activation timestamps.
  Focus completion coalesces transient native flags, supports callback redirection
  and exceptions, and releases pending content references after failures. Manager
  document cancellation precedes content callbacks; closed callbacks see detached
  content after layout cleanup and can reopen it elsewhere. Tests cover derived
  document/tool overrides, native flags/factory operations, item commands,
  floating hosts, absent base calls, exception recovery, and retained-root lifetime.
- Document close cancellation, tool hide/restore, hidden collection, active
  content, and title/content/capability forwarding.
- Content identifiers and callbacks match 208 WPF observations across 68 document/tool
  scenarios: null defaults, named-control fallback after notifications, explicit and
  empty identifiers, content identity, callback redirection and exceptions, bound
  identifiers, and independent container inputs that forward null without conversion.
  Native content changes use the same name fallback. Activation hooks match 176
  observations across 88 scenarios, including requested values after redirection,
  nested callbacks, suppressed base calls, exceptions, and deactivation that retains
  root activation and selection. Shared probes run in unit tests and package consumers.
- `LayoutRoot.ActiveContent` and manager active-content notifications span the
  primary and floating native roots while retaining each root's own focused
  item. Model-only activation also deactivates the preceding pane before its
  view is loaded. `LastFocusedDocument` includes tools in document panes and
  notifies both affected `IsLastFocusedDocument` properties. Activation, source
  root-specific focus clearing, window reactivation, callback redirection,
  hierarchy removal/replacement, unrelated native roots, and deferred floating
  navigator selection are covered by Headless tests.
- Manager active-content values retain explicit objects and publish layout models
  on layout-driven activation. Payload/identifier/empty-content lookup, duplicate
  matches, null defaults, layout replacement, protected hooks, callback redirection,
  exceptions, styles, and two-way/explicit-update binding callbacks are covered by
  the shared WPF probe. Its 280 rows cover 110 scenarios against one unchanged
  WPF reference fixture, including the eight former binding/style differences.
  The package consumer runs the same operations. History notifications preserve
  WPF's reentrant order, including the outer transition's history and independent
  item flags. Opt-in framework metadata implements assignment, current-value,
  clearing, and style-priority behavior without private binding-mode inspection
  or reflection. One-way local assignments replace their binding; two-way local
  assignments retain it and update their source.
- `PreviewFloat`, `Floated`, `PreviewDock`, and `Docked` bubble through the
  manager and its ancestors with the public layout content as `Source`.
  Protected notification hooks support derived managers. Direct model docking
  reports its public insertion and activation phases; native drag execution uses
  operation scopes so internal split/group moves do not duplicate notifications.
  Native validation, tab reordering/group commands, and reuse of an existing floating pane are
  silent. Tests cover timing against live native ownership, group selection,
  grouped redocking, preview ordering before auto-hide unpinning, handled routing
  without cancellation, capabilities,
  reentrant/throwing handlers, layout replacement, and disposed-scope lifetime.
  Dock's optional model-only factory observer brackets an executed operation
  before its first collection mutation and releases its scope after failure.
- Anchorable edge placement with `AddToLayout`, writable visibility, cancelable
  hiding callbacks, tab-index restoration, and `Show` insertion strategies.
  Hidden auto-hide tools retain their group and original pane across XML saves.
- Explicit `Close`, `ToggleAutoHide`, and `DockAsDocument` follow the upstream
  model contract independently of command capability flags. Protected native
  close/pin operations preserve cancellation, lifecycle events, ItemsSource
  removal, ownership, and cleanup without changing policy for reentrant calls.
  Closing hidden content removes its native hidden-list entry; pinned removal
  unpins as cleanup even when the pin command is disabled.
  Default command queries retain native policy/last-child checks and refresh on policy
  replacement. `Float` restores a previous floating pane with `CanFloat=false`,
  activates even after a rejected new float, and can refloat existing floating
  content while retaining its original docked return pane. Headless and external
  package-consumer tests exercise these differences from UI command eligibility.
- Direct model `Dock` and new-window `Float` require a root and manager; detached
  calls fail before selection or movement. Existing floating return panes can be
  restored without a manager. Previous-container insertion publishes the requested
  index immediately, rejects negative indices, and supports a destination in
  another layout. Completion reads the current destination manager. A suppressed
  `InternalDock` does not add activation; `Dock` clears the floating flag after
  `Docked`, and `DockAsDocument` completes before explicit activation. Garbage
  collection retains unreferenced return metadata and clears foreign references
  without rewriting the saved index/identifier. Headless cases cover collection
  notifications, missing context, external overrides, callbacks, and return history.
- Group minimum properties publish only the `DockMinWidth`/`DockMinHeight`
  compatibility names, including native assignments. Equality precedes negative
  validation; repeated `NaN` and reentrant assignments retain both public
  notification phases. Argument errors and callback failures preserve their
  upstream phase and completed state. XML restores remain publicly silent.
- Observable document/tool sources, source replacement, layout replacement,
  insertion strategies, and source removal from visible, hidden, and pinned
  locations. Compiled bindings and data templates render ReactiveUI ViewModel
  contents; source subscriptions do not retain unused managers.
- Stable document/tool item containers with live model metadata, selection,
  activation, content presenters, and replaceable ReactiveUI commands. Commands
  cover close/float/activate/dock-as-document, document sets, document splitting
  and adjacent-pane moves, plus tool hide/auto-hide/dock. Native capabilities and
  close/hide cancellation are honored. Containers survive moves and floating;
  removed nodes and replaced layouts detach their containers without disposing
  application commands. Compiled bindings and pointer input exercise container
  content and commands. Avalonia `IsVisible` hides tools or closes documents;
  WPF's separate hidden/collapsed distinction has no direct Avalonia equivalent.
- Manager container styles use Avalonia `ControlTheme`, with explicit-style
  precedence over `LayoutItemContainerStyleSelector`. The selector counterpart
  is `AvalonDock.Controls.StyleSelector`, returning `ControlTheme` from
  `SelectStyle(object?, AvaloniaObject)`. Compiled bindings synchronize titles,
  identifiers, capabilities, and application commands through the container's
  `Model`, including two-way updates. Containers use the manager's logical
  resource scope, support base themes and dynamic resources while floating,
  and reselect themes when content types change. New source entries and hidden
  tools receive styles; XML replacement releases old containers and bindings.
  Replacing or clearing styles preserves native state and application command
  ownership. A retained ViewModel does not keep an unused styled manager alive.
  Derived managers retain the native default control theme.
- Native document close buttons, tool title-bar close/pin buttons, document/tool
  context menus, auto-hide menus, and tab middle/double-clicks use current item
  commands, including compiled application command bindings. Runtime replacement,
  command enablement, native capability policies, and docked/floating rendering
  are covered by pointer tests. Floating-tool close commands preserve cancellation
  and overrides before native window closing. Tool close buttons fall back to
  hide when closing is disabled. Layout/factory methods remain direct operations.
  Default menus release closed managers with application commands and themes
  retained. Floating document docking returns to the previous document pane or
  selects/creates a main document pane through native factory movement.
- Manager `DocumentContextMenu` and `AnchorableContextMenu` use custom Avalonia
  menus with compiled `LayoutItem` bindings in document/tool tabs, title bars,
  auto-hide strips, and floating hosts. Tools docked as documents use the
  document menu. Keyboard requests use the active item and respect child menus,
  including canceled child requests. Default menus are per manager; explicit
  null, replacement, cancelable opening, active-tool changes, and layout/view
  detachment are covered. Submenus reopen with application bindings and theme
  changes preserved. Generated presentation and focus references are released
  using public Avalonia APIs: retained application menus do not keep closed
  docked/floating managers alive. Focus returns after closing or canceled opening.
- Public `DropDownButton`, `DropDownControlArea`, `ContextMenuEx`, and `MenuItemEx`
  use Avalonia menu input and themes. Coverage includes compiled XAML, external
  protected overrides, generated/explicit containers, source snapshot refresh,
  icon selector precedence/null results, command activation, checked state,
  cancellation, replacement during opening, reentrant reopening, bound existing
  menus, keyboard activation, child menus, and view detachment/reattachment.
  Preview release leaves `Handled` unchanged and opens before nested/child menu
  handling. Repeated protected button clicks retain the open popup and normal
  two-state/three-state toggling; commands run after `Click`, including a handled
  event or an explicit disabled-control hook call. Temporary context-menu
  attachment is removed before popup opening, preserving ordinary menus,
  bindings, application release handlers, and live resource changes.
  Closing preserves application icon/header bindings and retires old presentation
  and compositor references; retained menus with independent templates release
  closed controls/managers. Application-owned compiled templates retain their
  declaring XAML resource scope. `DropDownButton` collides with Avalonia's control
  name, so ordinary Avalonia XAML uses a prefix; a dedicated file with the
  historical URI as its root default namespace supports the unprefixed form.
  The other menu controls and layout API remain unprefixed in Avalonia XAML.
- Native Dock content controls display `LayoutItem.View`, retaining presenter
  identity between docked and floating hosts. Manager `LayoutItemTemplate` and
  `LayoutItemTemplateSelector` support compiled templates, explicit-template
  precedence, null-selector fallback, runtime replacement, content type changes,
  and recycling within the selected template. The selector framework counterpart
  is `AvalonDock.Controls.DataTemplateSelector`, returning an Avalonia
  `IDataTemplate` from `SelectTemplate(object?, AvaloniaObject)`. Inline controls
  retain native identity and handling. Retained empty presenters release closed
  models and managers; XML replacement releases old presenter content.
  Pane templates pass public content wrappers to native deferred presenters,
  preserving logical ownership when a source-bound manager is detached and
  reattached with its data context retained, including floated content.
- Public `LayoutDocumentControl` and `LayoutAnchorableControl` are used inside
  native docked/floating panes and auto-hide previews. Their model properties
  resolve the existing read-only `LayoutItem`, and their compiled themes present
  its same `View`. Document controls also host tools docked as documents.
  Model replacement updates subscriptions; public controls copy enablement only
  when the model notifies a change, while native presenters also initialize it.
  Disabling active content selects the first enabled sibling or clears selection.
  Protected model/container hooks and actual focus/left/right pointer routes are
  covered, including derived themes, nested managers, delayed navigator focus,
  and the floating-host focus exception. Pointer ownership uses the source's
  nearest parent manager, rejecting standalone, foreign, nonvisual and unrooted
  presentations. Content preview hooks accept handled events; pane press hooks
  require unhandled events. Focus hooks activate their model or pane selection
  without pointer ownership or explicit-call enablement filtering, matching
  upstream. Six pane cases failed before this correction; the broader input
  matrix also exercises real nested-manager focus and document preview hooks.
  Base input hooks read the current public model even when an application
  suppresses the base model-change hook; replacement and null assignments are
  covered independently of the retained item binding.
  The keyboard hooks use Avalonia's
  bubbling `FocusChangedEventArgs`, not WPF preview-focus routing; pointer hooks
  use tunneling `PointerPressedEventArgs`. Native control recycling preserves
  content views. Layout replacement closes document lists even when their native
  pane/button is recycled.
  Weak model subscriptions allow unused controls to be collected. Detachment
  releases an unused generated control's native parent, including collapsed tool
  panes; retained closed controls do not keep their models or manager alive.
- Public `AnchorablePaneTitle` exposes a tool `Model`, the existing read-only
  `LayoutItem`, a protected setter, and model/input hooks with Avalonia event
  arguments. Compiled default and historical XML namespaces and custom themes
  work. Native tool chrome uses the control for selected docked/floating titles
  and auto-hide previews, preserving manager template/selector behavior.
  Custom title input activates the tool, floats its visual pane or individual model
  through the native factory, and starts the native host drag. Existing single-pane
  floating windows are reused; native grips do not start duplicate drags.
  Tests cover cancellation, restricted capabilities, refused multipane moves,
  interrupted presses, interactive children, and manager ownership. Shared weak
  model/container bindings handle reentrant model changes, XML replacement,
  and collection of unused controls and retained closed titles. Native
  `HostWindow.TryBeginMoveDrag` exposes the existing drag lifecycle with factory
  cancellation and pointer-release completion. Platform movement remains an
  integration-validation requirement.
- Public `LayoutDocumentTabItem` and `LayoutAnchorableTabItem` use a
  `LayoutContent` model, the existing read-only `LayoutItem`, and manager header
  templates/selectors inside native tabs. Compiled XAML supports both namespaces;
  tools docked as documents retain their tool item in a document tab.
  Custom rows activate, invoke current document close commands, reorder the
  live pane, and float one tab through native window-drag cancellation. Native
  tab containers keep their drag preview/drop and command handling. `CanMove`,
  enablement, drag capabilities, and pane/group `CanRepositionItems` are checked
  by custom input and the factory's optional native `IDockItemDragPolicy`.
  The policy preserves ordinary Dock defaults and outside dragging when only
  reordering is restricted; changes during a gesture cancel pending moves.
  Tests cover protected model/container and Avalonia pointer hooks, focusable
  children, bound collection reordering, interrupted drags, XML replacement,
  manager transfers, and collection of unused and retained closed tabs.
  The activation hooks match upstream independently of drag/source/handled
  filtering. Document middle-click uses the current item's `CloseCommand`,
  including without a current model, and leaves `Handled` unchanged. A custom
  command may execute with `CanClose=false`; explicit native policy precedence
  still applies, and the default command retains its capability checks.
- Public `DocumentPaneTabPanel` and `AnchorablePaneTabPanel` lay out native
  horizontal tabs and custom bound rows in either XML namespace. Document tabs
  retain natural widths, suppress overflow, and move a selected hidden node to
  the front through the live pane. Native document-list activation reveals it.
  Tool headers compress with equal measurement caps and clip to their slots.
  Tests cover measured/arranged widths, collapsed/empty panels, floating hosts,
  selection without activation, model replacement, protected sizing/input hooks,
  binding restoration, reentrant removal/detachment, and weak subscriptions.
  Since Avalonia lacks WPF Hidden, overflow preserves measurement and `IsVisible`
  while temporary opacity and input layers suppress rendering and focus.
  Native drag input excludes noninteractive targets during movement and release.
- `DocumentHeaderTemplate` and `AnchorableHeaderTemplate`, plus their selector
  properties, apply compiled templates to native document/tool tabs and
  auto-hide strips. Templates and selectors receive public layout nodes;
  selectors take precedence and coerce explicit templates to null. Runtime
  changes update docked and floating headers, and title bindings observe native
  metadata changes. Tools docked as documents keep their `LayoutAnchorable`
  wrapper in the document header selector. Headless clicks verify native tab
  selection, activation, and auto-hide previews. XML replacement detaches old
  headers; retained closed headers release models and managers.
  Native tool chrome also releases its shared flyout's button reference when
  detached and restores the current menu binding on reattachment, including
  menu replacement or removal while detached.
- `AnchorableTitleTemplate` and `AnchorableTitleTemplateSelector` apply compiled
  templates to native tool chrome for docked and floating panes and auto-hide
  previews. They receive the selected public `LayoutAnchorable` and its presenter,
  independently of tab headers. Selector precedence, null fallback, runtime
  replacement, native tab selection, metadata updates, context-menu input, and
  close commands are covered by Headless tests. Source removal and XML replacement
  detach old title presenters; retaining them does not retain a model or manager.
- `DocumentTitleTemplate` and `DocumentTitleTemplateSelector` render the public
  root document in a floating document window's client caption. Compiled
  templates, selector precedence, null fallback, runtime replacement, and native
  title metadata are covered. The native host retains its window state and drag
  lifecycle; caption buttons use maximization and the current item close command.
  Headless tests cover drag cancellation, button input, double-click maximization,
  context menus, close capabilities and cancellation, XML/root replacement,
  reattachment, and retained-caption lifetime. Dock's drag helper also clears a
  pending press when the next move reports no left button, preventing a hover
  after a click from starting a window drag.
- Document tab strips expose a native document list menu in docked and floating
  panes. Header templates/selectors and generated icon controls use the public
  layout content. Icon selectors receive the current icon and owning `MenuItemEx`.
  Header fallback, selector precedence,
  runtime replacement, default `IconSource`, and metadata changes are covered.
  Collection changes refresh `ChildrenSorted` without rearranging native tabs.
  Entries invoke the current item activation command and follow its enablement.
  Tools docked as documents retain their public tool wrapper. XML replacement
  and view detachment close old menus; retained generated headers and icons
  release removed models and managers.
- Layout cleanup retains the last document pane and referenced return panes,
  clears detached active content, and collapses redundant typed pane groups.
- Manager `FloatingWindows` and `LayoutFloatingWindowControlCollectionChanged`
  expose the actual typed native hosts and collection indices. Document/tool
  controls implement `ILayoutControl`, share their public floating models, and
  report native drag/maximization state. Document controls resolve the current
  `RootDocumentLayoutItem`; tool controls use compiled single-pane selection
  with `LayoutItemFromLayoutModelConverter` and preserve application overrides.
  Both XML namespaces resolve the converter. Tests cover native host identity,
  cancellation, docking, XML replacement, detach/reattach, nested native locator
  requests, exception cleanup, reentrant collection changes, and protected
  constructor content immutability, and retained-empty-collection lifetime. Dock's optional `IDockWindowHostFactory`
  enables model-aware creation while preserving ordinary/native ID locators.
- `CreateFloatingWindow` moves content through the native factory and returns a
  new unshown typed host with per-request content immutability. Tests cover
  document/tool geometry, loaded-content replacement and native-layout restoration,
  standalone panes, unattached/auto-hidden tools, cross-manager transfer, creation
  before the main view loads, native registration and explicit presentation,
  nested creation/float options, capability checks after preview, return panes,
  and exception/layout-replacement cleanup. Floating views inherit the manager's
  native host factory even when shown before the main view. The package consumer
  creates, shows, protects, and docks an explicit window.
- Command converters resolve the current manager-owned item and its activation,
  hide, or auto-hide command. Headless pointer tests execute those commands
  through compiled unprefixed XAML, including floating/hidden tools. Converter
  tests cover command overrides, detached/replaced layouts, wrong input types,
  document/tool distinctions, side angles/orientation, null-to-do-nothing,
  visibility and its inverse, multi-value hide visibility, and reverse methods.
  Visibility converters use Boolean `IsVisible` and Boolean parameters in
  Avalonia; the multi-value converter also preserves the public array API.
  The separate NuGet consumer compiles all nine additions and exercises command
  input, visibility updates, and target retention through package references.
- Tool hosts expose `CloseWindowCommand` and `HideWindowCommand`, with current
  per-item commands and parameter forwarding. Protected `CanClose`, `CanHide`,
  and `DoHide` participate before the public window `Closing` event. Tests cover
  capabilities, null/disabled/replaced commands, membership changes, snapshot
  execution, content cancellation, native document close and tool hide fallback,
  unprefixed compiled bindings with Headless input, and subscription/lifetime
  cleanup without disposing application commands. Direct `Execute` delegates
  to the item commands as upstream does; callers first query `CanExecute`.
  The hooks and tool commands use the current virtual `Model`, selecting the
  appropriate descendant type even when it is a root, pane, or leaf. Detached,
  managerless, and null overrides return false from capability queries.
  Model changes refresh command subscriptions; reentrant execution retains its
  starting snapshot and resolves each command live. Typed captions and document
  container lookup retain the constructor model. Closing marks cancellation
  before hide hooks and preserves their changes, including when a hook throws.
  Internal closure also tolerates a null overridden model.
- Hiding tool groups and retaining floating return panes preserve the actual
  host instead of closing/recreating it. Visibility restoration keeps its
  resources, content, geometry, container identity, and native registration.
  Tests cover direct hiding, window-wide commands, native-close hide fallback,
  partial cancellation, never-shown explicit creation, XML replacement, view
  detach/reattach, and collection/command lifetimes after disposing hidden hosts.
  Initially hidden layouts receive unshown controls. Dock's native host avoids
  duplicate factory registration when Avalonia raises `Opened` again after `Hide`.
  Native Headless tests also verify that retained hidden windows and external
  surfaces cannot become drop targets or receive bring-to-front activation.
- Protected `CloseInitiatedByUser` records internal close requests independently
  of native tracking/visibility. Derived-control tests cover initial state,
  user cancellation, hiding/restoration, model cleanup, native exit, layout
  replacement, view unload/reattachment, and a canceled internal request.
- Canceled internal cleanup preserves the floating model, host reference, tracking,
  and manager/native registrations. Collection removal, clearing, replacement,
  and cross-manager transfer respect native/factory vetoes. Garbage collection
  retains canceled empty hosts and can retry without looping indefinitely;
  completed content close/dock operations are not rolled back. Tests cover
  retries, nested closed-callback cleanup, independent host/content closing,
  and one removal notification. Dock's optional `IHostWindowCloseState` also keeps
  ownership intact for managed hosts. Model tests cover legacy hosts, previous
  tracking state, reentrant or throwing exits, and host replacement in callbacks.
- `AllowMovingFloatingWindowWithKeyboard` defaults to false and supports
  compiled XAML bindings and literal assignments from another assembly. Its
  public CLR setter is an explicit Avalonia adaptation: literal XAML calls the
  setter directly, whereas WPF can write the underlying dependency property
  with its private CLR setter. Floating hosts move by 25 device-independent units
  on unhandled arrow keys, using desktop scaling. Headless key input verifies
  both host types, runtime toggles, unrelated/handled keys, modifiers without
  swallowing the event, per-manager state, hide/show, view reattachment, and
  native/XML position persistence.
- `ResizeBorderThickness` defaults to 10 on each edge and drives eight themed
  resize regions on both native host types. Regions expose Avalonia's native
  decoration roles, cursors, and resize-drag calls. Headless tests cover all
  edges/corners, nonuniform values, compiled literal/style bindings, runtime
  updates, zero thickness, fixed-size/maximized/fullscreen hosts, center content
  input, and hidden-host restoration. The floating template retains native Dock
  content with a border/inset that keeps ordinary tab clicks clear of the resize
  regions. Native system decorations remain platform-controlled; real-platform
  resize and hit-test parity still require integration validation.
- Document and tool floating models expose native Dock windows through an
  observable `FloatingWindows` collection. Native single-document float/dock
  and tool floating operations preserve content wrappers and public ownership.
  Compiled floating XAML renders in native hosts; collection removal, manager
  detachment/reattachment, and layout replacement preserve the layout contents.
  Floating position, size, and maximization synchronize between native hosts,
  Dock windows, and all public content/group models in the floating tree.
  Floating hosts resolve compiled content templates in the manager's current
  scope, including ancestor templates, and preserve template recycling without
  retaining an unused manager.
  Public and native single-content floating operations preserve tool return
  panes and tab indices. Docked tools can reuse populated former floating panes;
  empty floating return panes are collected. Valid return locations survive XML.
  Protected docking fallbacks invoke layout
  insertion strategies when there is no previous pane.
  Whole tool panes and typed tool groups retain their nested docked return
  locations, tab order, selected content, and docked sizing. Batch floating
  checks all child capabilities, and moving an already-floating group retains
  its docked history. Repeated XML saves distinguish copied native pane IDs
  and restore visible contents without stale native floating ownership.
  Native floating host close requests honor window/factory cancellation and
  close capabilities. Tool groups hide when they cannot close but can hide,
  retaining their floating return pane for `Show()`.
- Floating defaults, property order, callback-visible root/parent state, child
  replacement/removal/restoration, visibility, nested pane counting, and XML reads
  match 300 WPF observations across 150 scenarios. The same probe runs against
  the package consumer. Subclasses can suppress changing or changed notifications
  independently of explicit document/visibility events. Empty floating XML keeps
  existing children and bounds; empty tool element forms preserve WPF's distinct
  visibility behavior. Single-slot container operations match upstream Release
  behavior for mismatched old children and incompatible replacements. Adoption
  checks actual membership before removing a retained former child. Floating
  captions observe visibility separately, preserving hidden-tool item bindings
  without adding public model notifications.
- Auto-hide sides and groups project Dock's native pinned lists, preserving
  public group ownership through pin, preview, unpin, collection replacement,
  and reordering. Headless pointer input opens the sample's Agenda strip.
  Explicit pins create individual groups and preserve their strip position
  across hide/show. Floating tools pin in the manager's root while retaining
  their floating return pane through repeated previews and XML recovery;
  commands inspect current ancestors instead of the retained floating flag.
  Restoring selects without explicitly activating, creates missing edge panes
  with upstream orientation/minimum rules, and redirects hidden return-group
  references even while siblings remain. Closing redirects after cancellation;
  hiding removes native pins/previews with the pin capability disabled.
- Compiled Avalonia default and Xceed XML namespaces; headless pointer tab selection and text input.
  Pixel/Auto/star pane dimensions translate to Dock proportions using each nested
  or floating panel's actual available space. `GridSplitterWidth` and
  `GridSplitterHeight` default to six and update native splitters at runtime,
  including orientation changes. Collapsed return panes and their hidden
  splitters do not consume space. Headless tests cover star ratios, pixel sizes,
  pointer drag persistence across resizing, XML replacement, reattachment,
  independent managers, and collection of replaced panels and closed hosts.
  All-pixel splits preserve unused space and fixed lengths beyond a constrained
  viewport. Minimums default to 25, reject negative values, notify through public
  aliases, and redistribute remaining star space without changing requested
  lengths. Native preview resizing commits on release, cancels on capture loss,
  and preserves the combined weight of a resized star pair. Mixed pixel/star
  resizing uses the upstream pair-weight rule. Both axes and floating panes are
  covered. `Auto` uses the existing pane's intrinsic content/chrome size and reacts
  to content changes, selection, minimums, and XML restoration. Nested plain panels
  measure unconstrained stars intrinsically and retain their natural minimums when
  arranging. All-auto splits preserve unused space and can exceed the viewport;
  collapsed panes consume no space. Native preview commits turn auto lengths into
  pixels. Both axes, floating groups, group rebuild normalization, reattachment,
  and discarded sizing-view collection are covered.
  Replacing presenter styles or floating-window resource themes restores layout
  inputs from unchanged model proportions without waiting for another model event.
  Native Dock retains its default splitter thickness; its proportional panel
  refreshes splitter dimensions on thickness and orientation changes. Protected
  measurement, assignment, dimension, and resize policies let the adapter reuse native layout
  and pointer handling while ordinary Dock retains its normalization behavior.
  The native document create-button presenter no longer parents an Avalonia
  model into its logical tree, so retaining an old model releases discarded grids.
- XML for docked, floating, auto-hide and hidden layouts, callbacks, content identities,
  selected documents, basic metadata and constraints, file/stream/text APIs,
  malformed input rejection, and atomic manager layout replacement.
  Source collection notifications are suspended during XML content callbacks
  and resume after successful or failed restoration, including nested serializer
  scopes through the protected deserialization hooks.
  Content, pane/group, root, and floating `IXmlSerializable` methods share the
  explicit codecs. Standalone readers preserve existing native model identity
  and application content, and restore tab selection before a factory is attached.
  Content readers apply document/tool attributes before base metadata, select
  immediately after the title, and advance the original reader once. Custom
  overrides can read child elements after the base call; conversion or callback
  errors retain preceding assignments and the failing attribute position.
  Persisted last-focused flags do not activate content or update timestamps.
  Floating flags round-trip as metadata without creating windows; later native
  docking-state changes update the facade before native-state observers run.
  Groups, roots, and floating windows retain the caller's live XML reader. Root reads replace sections while retaining
  the manager's native root and factory. Without a callback, missing documents
  close and tools hide while retaining their return panes. Callback cancellation
  requests closure and preserves content closing-event vetoes. Unresolved null
  placeholders remain when a callback leaves both content values null.
  Recovery separates captured document/tool models, reads their live identifiers,
  skips already-populated content, and runs tool callbacks before the document
  snapshot. Callback code owns icon restoration; default recovery copies the
  previous icon. Nested virtual `WriteXml` overrides run through a shared writer
  across docked, hidden, pinned, and floating nodes. Failed overrides preserve an
  existing save file. `XmlLayoutSerializer.KnownTypes` registers custom XML names
  and typed factories, preserving nested read/write overrides without reflection.
  Registrations are snapshotted per operation; implicit assembly scanning is absent.
  Return-container restoration distinguishes an explicit empty pane ID from an
  unset ID, resolves all previous references before initial references, and rejects
  missing references before content callbacks or manager replacement. Pane IDs
  take precedence over native group-extension IDs and survive repeated saves.
  Group XML restores dimensions, floating metadata, IDs, tool-pane names, and
  document header flags without public setter notifications or size propagation.
  Native minimum values still update their Dock models; reentrant public changes
  and later edits retain normal notifications. Attributes apply in inheritance
  order, preserving earlier assignments when a later conversion fails. Stored
  minima bypass the public setters' negative-value check, matching upstream field
  restoration. Orientation accepts case-insensitive names and numeric enum values; invalid
  strings and overflow retain the enum parser's exception types.
  A self-closing empty group recomputes visibility; an explicit empty start/end
  pair retains the existing state. The 58 group-metadata cases and external package
  probe cover these contracts. The 51 content-metadata cases and external consumer
  probe cover direct and registered content reads. Numeric writes use invariant CLR double spellings so nonfinite values can be
  read again; return indices use current-culture formatting and parsing. Tool
  panes defer fallback selection until a successful XML read, while explicit
  selections and application callbacks remain immediate. Failed reads leave
  fallback disabled until a successful read, including successful nested reads.
  These cases cover built-in panes and registered application subclasses.
  Groups, roots, and floating windows now use live readers, preserving original
  depth, line information, callback positions, and following siblings. Direct
  reads retain preceding changes after later errors. Root sections stage children
  before attachment, replace sides before reading, and clear floating/hidden
  collections before reading replacements. Successive floating children replace
  the previous root and restore geometry without changing outgoing-child metadata.
  Deserialization wraps model parsing failures at each nested serializer boundary;
  content-recovery callbacks and reference fixup retain their original errors.
  Forty-three streaming cases and an external consumer cover these contracts;
  five package probes reproduce the preceding reader and error-boundary defects.
  The earlier floating test that rejected two document roots was corrected: each
  child replaces the previous one, so the last document remains.
  Forty-seven write-contract cases cover explicit inheritance dispatch, attribute
  order, default omission, CLR boolean spellings, whitespace-only content strings,
  writer callbacks and failures, and return identifiers. Unused containers no
  longer receive generated IDs. Explicit empty pane IDs remain supported, native
  group references receive IDs, and unsupported return-container kinds are omitted.
  Nine concrete/base write overrides now have matching source-presence keys.
  Seven group read overrides now follow the class inheritance chain; additional
  interfaces do not cause unrelated metadata restoration. Seventeen regression
  cases cover direct/registered readers, callback order, suppressed base calls,
  external `LayoutGroupBase` construction, native ownership, and notification hooks.
  Three external reader probes reproduce the preceding package defects; a separate
  compile probe reproduces the missing protected base constructor. The implicit
  upstream constructor is not represented by the source-presence inventory.
  Thirty-five writer-context cases cover nested model/writer errors, live caller
  namespaces/settings/XML context, partial output, callback ordering, child and
  outer flush boundaries, retained text/stream writers, and byte/text encoding.
  Seven external probes reproduce the preceding package defects. Group and
  floating child serializers use empty namespaces and flush at their own boundaries;
  direct root sections and hidden tools add no child serializer error wrapper.
  Text/stream wrappers remain usable after success or failure without closing the
  caller's output. Stream/file writes use UTF-8 without a byte order mark.
  Root sections recognize only the exact `Vertical` spelling and consume known
  children that do not have the section's required model interface. Registered
  readers run for discarded children, but content recovery does not see them.
  Thirty-one root cases cover spelling, direct/whole-layout restoration, filtering,
  section staging, and the retained native metadata extension. Fourteen interface
  cases verify concrete membership and class-based empty-group visibility dispatch;
  generic groups no longer make sides/anchor groups into panes or panel elements.
  Eight visibility inheritance cases cover both concrete parent recomputations,
  reparenting, errors, and suppressed base calls. Seven external probes reproduce
  the preceding package defects. Remaining XML work includes broader root-format,
  culture, and namespace inputs. Compact roots with omitted empty sections and persisted native root-panel
  metadata, native hierarchy validation, and preserving existing files on model
  write failures remain adapter extensions to the upstream format.
  Assigning a return container raises only its changed notification, then initializes
  an unset pane ID using the reference left by application callbacks. Identifier
  metadata is quiet; changed return indices raise only changed notifications.
- Content floating dimensions initially use zero, allowing Dock to choose a valid
  native size when a window is created. Titles and tool-pane names initially use
  null. Equal title assignments are quiet, and tooltip/icon changes use reference
  identity. Changed-only content metadata, pane maximization, and header flags
  use upstream notification contracts;
  tool-pane names are separate from Avalonia's immutable styling name and remain
  mutable after native styling, with changed-only notifications and silent XML reads;
  native capability changes publish `CanMove`, `CanAutoHide`, and
  `CanDockAsTabbedDocument` through the public models. `LayoutContent.CompareTo`
  and `LayoutDocumentPane.ChildrenSorted` use application `IComparable` content
  before falling back to titles, preserving application comparison exceptions.
- Xceed Live Explorer 3.7.0 sample adaptation with its Ms-PL license, source
  notice, passive views, ReactiveUI commands, and save/load integration tests.
- `NavigatorWindow` presents live document/tool containers in a compiled template.
  Ctrl+Tab works from the manager and both native floating host types. Tests cover
  stable activation ordering, filtering, snapshot and live metadata behavior,
  upstream initial-selection and keyboard rules, pointer input, command overrides
  and availability, nested editor focus after delayed/budgeted presentation in
  docked and floating documents, native floating-host activation after modal
  closure, cancellation on replacement/deactivation/detachment/new focus (including
  handled events), collection while unrealized content is retained, labels in both
  XML namespaces, public/protected template and selection hooks, empty/custom
  lists, cancellation, reentrant/failing creation, layout replacement, view
  detachment, collection of closed navigators, and hidden-window owner cleanup,
  including canceled owner closure and reassigning or clearing ownership on show.
  The package consumer exercises
  unprefixed label styles, the public template hook, keyboard list navigation,
  native activation, editor focus, and desktop lifetime shutdown after multiple
  hidden navigators are cleaned up when their owner closes.
- `Themes.Theme`, `DictionaryTheme`, `GenericTheme`, and the manager's styled
  `Theme` property/protected hook. Shared dictionaries support live resource and
  theme-variant changes across managers, floating windows, and navigators, including
  hidden windows, external dictionary ownership/transfer, reentrant updates,
  replacement/removal, close cancellation, and collection while a theme is retained.
  Closing a floating document/tool, navigator, or overlay during resource removal,
  resource creation, or replacement does not attach resources after disposal.
  Canceling that close preserves the replacement theme and later updates.
  The compiled `Themes/Generic.axaml` dictionary separates resource keys
  from selector styles and can be included by URI from consumer XAML. Tests verify
  both XML namespaces, compiled bindings, view-scoped adapter styles without
  application-wide Dock resources, the inherited Avalonia control-theme
  property, invalid configurations, and the built-in URI.
  `DockingManager.ThemeResources` resolves custom URIs through registered compiled
  dictionary factories for the manager and its auxiliary windows. Unknown URIs
  fail explicitly; runtime type discovery and XAML text loading are not used.
- Public `LayoutCachePaneControl`, `LayoutDocumentPaneControl`, and
  `LayoutAnchorablePaneControl` expose the live pane and child collection while
  their default themes retain native Dock presenters and cached item views.
  Manager pane styles use `ControlTheme` and update existing docked/floating
  controls. Both XML namespace forms, external hooks, empty/passive selection,
  explicit document activation, tool focus/pointer input, selection during child
  moves/removal, manager transfers, and weak publisher lifetime are tested.
  Presentation selection waits until child collection observers finish. Native
  compatibility strips allow an empty selection; ordinary Dock defaults remain
  unchanged. The protected `IEnumerator LogicalChildren` hook also supplies
  framework traversal; document panes expose upstream's empty enumeration.
  Grouped/mixed layouts use the additional native-backed floating model below.
- `LayoutDocumentPane.ShowHeader` controls the actual tabs, document menu, and
  separator through compiled template bindings, retaining the selected content
  view, focus, and activation. Pane policies remain independent through runtime
  themes, floating/docking, XML restoration, and reattachment. Both XML namespace
  forms and Dock's dynamic tab-strip visibility resource are tested.
- Native and custom public tab headers apply live `IsEnabled` and `ToolTip`;
  default tool headers include `IconSource` in docked, floating, and auto-hide
  locations. Custom header templates replace the default icon/title content.
- Public command resources provide `ResourceManager`, `Culture`, and all 21 keys
  in English plus 13 satellite cultures: `cs-CZ`, `de`, `es`, `fr`, `hu`, `it`,
  `ja-JP`, `nl-BE`, `pt-BR`, `ro`, `ru`, `sv`, and `zh-Hans`. Parent-culture and
  English fallback follow .NET's resource rules. The translations are authored
  for Dock. Compatibility menu labels, tool/document hints, auto-hide header
  conversion and floating maximize/restore hints use these resources. Dynamic
  resource overrides remain supported. Existing static XAML values are not
  refreshed by changing the culture override. The URI image converter loads
  local files and compiled assets, including relative base URIs.
- `LayoutGridResizerControl` uses native proportional splitter input and preview
  commit/cancel behavior. Its brush and opacity settings update an active preview.
  Auto-hide templates expose the same public control over native pinned bounds:
  manager width/height settings update the displayed host,
  every edge and right-to-left content supports preview/commit, and interrupted
  drags cancel. Native inline previews also resize. Size assignments clamp to
  the current minimum, preserve native bounds, and raise changing/changed events;
  negative minima are rejected. Headless tests cover input, live settings,
  cancellation, public model notifications, and retained-model lifetime.
  Ordinary native splitter defaults remain unchanged.
- `LayoutAutoHideWindowControl.HasFocusWithinCore` preserves the upstream base
  hook's constant `false` result. Avalonia's `IsKeyboardFocusWithin` separately
  reports actual focus. `AutoHideFocusHookTests` exercises focus on the preview,
  inside its content, and outside it without reflection or changing constructor
  accessibility.
- `DockingManager.LogicalChildrenPublic` exposes its virtual weak child
  enumeration: presentation controls, initial application controls, and lazy item
  views, excluding layout item containers. The framework uses the same hook and
  filters non-logical entries. Base property callbacks own membership changes.
  Floating `OnStateChanged` overrides synchronize maximization when
  they call the base hook. `IsMaximizedProperty` retains the upstream writable
  property contract through an Avalonia styled property; assignment and binding
  do not themselves change the native window state. `IsDraggingProperty`
  remains read-only, matching upstream's dependency-property key.
- `OverlayWindow` hosts the native selectors when floating docking adorners are
  enabled. An optional inherited native window factory preserves the existing
  hit-testing/drop engine and the inline/managed presentation paths. Public
  drop-area/overlay-area/target types and enums describe the existing controls;
  additional accessors capture physical screen-pixel geometry on demand.
  Manager themes reach overlay windows and are released on close.
  Snapshots use native selector registrations and attached operations. Custom
  templates with remapped parts or nested similarly named controls agree with
  native hit testing; template replacement clears old selector/indicator maps.
  The window template now includes the manager, tool, document and optional full
  document grids, all directional/center selectors, and `PART_PreviewBox`.
  An `IDockTargetPresentation` maps these parts into native hit testing and
  renders native operation state and preview geometry. Document groups expose
  only the center target. Native visibility policy controls the outer-target
  presentation. Explicit native templates and custom target themes retain
  precedence, and native-only window templates retain their fallback path.
  Template replacement detaches retired presenters; successful closure also
  releases composition children and the window-owned default theme so a cached native target does not
  retain the closed overlay. Canceling closure preserves the presentation.
  Theme changes that clear and restore the same template preserve the target and
  selectors even when Avalonia reuses its visual tree without applying the template again.
- Outer previews cover one third of the highest document group, or the parent
  layout panel for an ungrouped document pane. Ordinary split and center previews
  remain confined to the hovered pane. The optional native `IDockAdornerHostProvider`
  and read-only `DockTarget.TargetMargin` keep selectors over that pane while
  expanding the surrounding preview. Inline, native-window, and managed-window
  paths track layout/window changes and scaled controls; local/global managed
  overlays keep separate entries. Detachment/removal clears geometry subscriptions,
  the adorned-element reference, theme and inset, and the helper can be reused.
- Manager and floating `OnInitialized(EventArgs)` hooks bridge Avalonia's actual
  initialization. The manager's public `OnApplyTemplate()` refreshes public
  presentations while retaining unchanged models. Floating `OnClosing(CancelEventArgs)`
  shares native arguments with the Avalonia overload and event, preserves native
  factory/window vetoes, and supports canceled cleanup retries.
  Overlay BCL/Avalonia closing hooks also share arguments and preserve their
  projections until closure succeeds. Protected typed-host and overlay
  constructors support application subclasses through native window factories.
- `ShowSystemMenu` defaults to true and compiles in both XML namespaces.
  Native Windows caption right-clicks prefer custom document/single-pane tool
  menus, including canceled menu openings, before consulting system fallback.
  Signed native screen positions are preserved. Floating `FilterMessage`
  hooks register with Avalonia from the first opening until close; caption
  activation leaves movement and unhandled messages to the native host.
  A composed Windows menu service uses source-generated interop, preserves
  native menu ownership, and forwards selected commands to the existing host.
  Windows-specific decorations retain system-menu styles without duplicate
  caption buttons. `ShowSystemMenu(PixelPoint)` is an additional protected host
  override. Windows 11 ARM64 desktop checks cover real callback results,
  hide/show registration, custom/canceled menus, system-menu display, and native
  closing/hiding cancellation. Native menu selection through real pointer/keyboard
  input and broader desktop input/focus checks remain open.
- Native destination validation matches the upstream floating-host rules:
  document floating windows reject incoming drops; tool floating windows accept
  tools from the same manager. `AllowMixedOrientation` restricts document split
  targets to the parent group's axis when it has multiple visible children.
  Native indicators and execution consult the optional `IDockDropPolicy`;
  direct factory edits bypass it. Native splits within typed document/tool groups
  preserve their public group types, item containers, and floating ownership.
  Tests cover all drop directions, docked and floating sources, rejected drops
  without mutation, XML restoration, and redocking.
- Header/title/menu property callbacks are checked against 247 observations from
  the pinned original WPF implementation on Windows 11 ARM64/.NET 10. Coverage
  includes local values, compiled bindings, styles, subclass suppression,
  replacement/removal, explicit coercion, and cached menu fallback. Native
  rendering preserves an explicit template when a subclass suppresses the base
  selector callback. Local template bindings are replaced by the base callbacks;
  style bindings remain available. Registered compatibility properties use
  the framework's value policy for direct `SetValue` calls. The reproducible
  reference probe is in `build/AvalonDock.WpfTemplateOracle`.
- Retired menu template roots are disconnected from their composition parent,
  preventing retained icon presenters from keeping removed documents alive.
- `Window.Close()` checks current close commands without executing them and
  preserves the floating model/content when its host closes. Nonclosable tools
  hide before the canceled window `Closing` event; content `Hiding` cancels that
  hide. Successful layout replacement detaches old floating models, while view
  unload preserves them. Dock's optional `IDockWindowClosePolicy` skips native
  content cleanup and releases the host for later presentation. Native model and
  Headless tests cover cancellation, preserved ownership, and replacement hosts.
  The WPF probe records 168 observations; regression tests compare all 48 hook
  cases and before/synchronous/dispatched states for all 40 host scenarios.
  Internal closure is queued once. Documents retain their public floating parent
  until adoption, and their original root until successful host cleanup. Empty
  tool models detach before the queued close, leaving the public host entry even
  after closure; native tracking and factory ownership are released. Unload clears
  public host registration even if closing is canceled, and reattachment restores
  a surviving host. Additional tests cover retries, old queued hosts, replacement
  hosts, native registration cleanup, and public/native document adoption.
- Floating model overrides match all 180 additional WPF observations: 60
  document/tool scenarios with child removal, replacement, clear/restore before
  dispatch, cancellation, and suppressed property notifications. Document hosts
  observe `RootDocumentChanged`; tool hosts observe `RootPanel` and `IsVisible`
  property notifications. Removed root panels retain their public floating parent
  while detaching their native owner, and both public/native adoption replace the
  retained parent across layouts. Closed hosts unsubscribe from retained models.
  These rules use the existing native owner-change and host-lifetime contracts.
- Native grouped documents and generic/mixed panels preserve their public wrappers
  through `LayoutGroupFloatingWindow` and `LayoutGroupFloatingWindowControl`.
  Group floating and float-all record document/tool return panes, including an
  entire root panel. Empty return groups retain their model while their host is
  hidden. Their XML element is a Dock extension outside the upstream WPF schema.
- Typed floating initialization hooks request model subscriptions after their
  base call. Avalonia can initialize in its Window constructor; model-dependent
  work waits until the adapter receives the model. Suppressing the base hook
  suppresses those subscriptions. This does not change Avalonia's Initialized
  event timing into WPF's timing.
- 2,422 passing cases in `Dock.Avalonia.AvalonDock.Tests`, including 17 new
  full-preview cases for docked/floating tabs, per-manager settings, both XML
  namespaces, native/managed hosting, cancellation, and content registration.
  All 30 cases in `Dock.Avalonia.AvalonDock.ImageTests` pass using Skia with
  Headless. Twelve rendered-image cases verify original editor pixels through
  tab selection changes and whole-pane drags at 1×/2× scale, together with
  snapshot release; the other cases cover image decoding and HTTP/HTTPS transport.
  NuGet pack produces net8.0 and net10.0 assemblies, XML docs, and the package
  documentation with dependencies on Dock's Avalonia model, Fluent theme, and
  ReactiveUI commands.

## Template callback delivery audit

Existing content, header, title and document-menu presenters observe template
and selector properties independently, after the corresponding protected change
hook returns normally. Failed callbacks keep that provider's last delivered
value; completed nested property changes remain applied. An unrelated provider
update does not import a previously failed value. New presenters start from
current manager settings, and a reentrant outer notification does not rebuild
an already-delivered inner value. Document-header changes leave an unchanged
coerced menu fallback alone.

This ordering follows WPF's [`DependencyObject.NotifyPropertyChange`](https://github.com/dotnet/wpf/blob/a088618af985b17dfbc2327c7caed84f7758a2ff/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/DependencyObject.cs),
which invokes property callbacks before invalidating dependent bindings. The
pinned AvalonDock manager supplies the template/selector callbacks; its content
presenter binds the providers separately. This pass uses source contract review
and Avalonia Headless, without resuming the deferred Windows desktop checks.

Fifty-one new cases cover all six presenter kinds, both provider properties,
success/failure ordering, exceptions after nested template clears, independent
failed-value retention, new-view initialization, reentrant child identity, menu
fallback identity, captured selector arguments, and current/sticky menu selector
fallback. The focused suite passes 139 cases, including the existing recorded
WPF metadata observations. The external consumer reproduces premature provider
delivery against package commit `2aaacd47e`; its new probe verifies the corrected
behavior through public/protected APIs.

## Layout callback argument audit

`OnLayoutChanged` assigns ownership and attaches containers to the current
`Layout`, including when an override redirects the property before calling
base or replays earlier arguments. Pane/side controls, floating hosts, native
container lookup, and pinned previews also follow that current property.
Source removal/import still uses the supplied old/new roots; subsequent source
notifications update the current root. A foreign supplied new root keeps its
existing manager, and a null new argument skips imports and notification hookup.

The base hook removes one property/update registration from the supplied old
root before source removal. It registers the supplied new root only after
container attachment, source imports and presentation complete. Import failures
do not advance to panel/host setup or install new root listeners. Replayed
registrations preserve multicast ordering and remove only one occurrence.
Their publishers and manager endpoints are weak; native container cleanup
independently observes the presented root. Root notifications consult the
current layout, and supplied-root updates refresh current container commands.

Nineteen Headless cases cover before/after-base redirection, suppressed nested
callbacks, source imports versus live notifications, foreign/null arguments,
presentation and failure ordering, repeated/detached registrations, application
handler ordering, command refresh, native floating/pinned presentation, and
both listener endpoint lifetimes. The focused suite passes all 80 cases.
The external package probe covers redirection, replay, source routing, ownership,
and independent notification delivery through the public/protected API.

## Root element registration audit

Each completed container attachment registers a distinct pair of root element
listeners. Suppressed or failed style callbacks do not commit a registration;
initial document callbacks run before taking the tool snapshot, and membership
observation starts only after successful attachment. Multicast delivery preserves
application-handler ordering and excludes registrations added during an event
until its next delivery.

Layout replacement removes one registration from the current layout property
before attaching again. Retained old roots still notify the registry, which
refreshes current containers without importing foreign old-root content.
Returning to a root removes one of its registrations, preserving the rest.
Weak keys and listeners retain neither an obsolete root nor an unused manager;
stale listeners detach when their retained publisher next raises an event.
Native `Updated` observation remains scoped to the presented root.

Seventeen new Headless cases cover repeated calls, suppression, exceptions,
old-root notifications, returning to a root, reentrant/application-handler order,
suppressed layout transitions, initial document/tool callback order, failed
initial attachment, and both ownership lifetimes. All 80 focused cases pass.
The package consumer verifies registration multiplicity, old-root notifications,
foreign-content exclusion and returning-root behavior on both target runtimes.

## Container style callback audit

The style hooks reattach current layout contents and use the manager's current
style settings, including when replaying older change arguments. Clearing both
providers retains existing container themes; a non-null selector returning null
clears the theme. Retained themes follow replacement content after old typed
bindings are released. Default commands are cleared before selection and restored
after successful application, including when the theme itself does not change.
Application commands remain intact. A selector exception leaves defaults cleared
and preserves the old theme; reentrant style changes finish before the outer
selector's result is applied.

Style callbacks visit separate document/tool snapshots. Existing attachments
refresh registered items in registration order, including containers awaiting
deferred removal. Native membership additions also refresh current styles;
source insertion batches retain their separate registration timing. Twenty-two
new Headless cases cover these contracts, including suppression, exceptions,
current-value replay, repeated selection, source/native additions, content
replacement and deferred cleanup. The focused suite passes 122 cases. Package
consumers exercise the protected hooks and provider/command behavior externally.

## Source callback and weak registration audit

The source hooks detach `e.OldValue` and import `e.NewValue` using the manager's
current layout. They no longer substitute the last applied source or reread the
current property as the new argument. Twenty-nine Headless cases cover
before/after-base redirection, suppression, exceptions, captured callback replay,
layout replacement, suppressed layout transitions, failed initial imports,
replacement during an import batch, repeated registrations, in-flight shared
source delivery, and both manager and collection lifetime. Existing shared-source
expectations were corrected against WPF's listener implementation: each
attachment delivers independently, with documents taking precedence when the
source properties refer to the same collection.

Layout transitions disconnect the old root's notifications before removing its
source contents, preventing premature active-content updates. Two regressions
reproduced the incorrect ordering before its correction. Transitions detach the
current sources from the supplied old root and
attach them to the supplied new root. Initial batches retain their root and
register their supplied collection only after completion; live changes use the
current manager layout. Weak identity keys and distinct weak subscribers preserve
independent registrations without retaining either endpoint. The initial focused
run exposed a strong listener reference; after correction, all 246 focused cases
passed before the two additional repeated-registration lifetime cases were added.
The external package consumer exercises reentrant source callbacks, shared
collections, and repeated attachment/removal on both supported runtimes.
These are source-derived contracts; desktop checks remain deferred.

## Manager resource and timer callback audit

The pinned upstream protected hooks use their supplied change values. Manager
resource updates use `OnThemeChanged`'s new value, while existing and newly
created floating windows, navigators, and overlays read the manager's current
`Theme`. Tests exercise before/after-base redirection, suppression, exceptions,
and explicit callback replay with both non-null and null themes across all four
window types. Twenty of 24 cases failed before the correction. The coordinator
retains resource reentrancy and weak ownership; closed-window and canceled-close
coverage remains in the focused theme suite.

The initialized auto-hide close timer likewise applies
`OnAutoHideWindowClosingTimerChanged`'s argument instead of rereading the property
on each idle transition. Suppressing a base call retains its applied interval;
callback replay and redirection use the supplied interval and base-call order.
Initialization reads the current property, including configuration supplied
before loading. Six Headless cases cover these contracts; four of the first five
failed before correction. Package consumers exercise both callback paths.
These source-derived contracts do not add desktop validation.

## Manager property contract audit

The pinned upstream manager registers `Layout`, `DocumentsSource`,
`AnchorablesSource`, and `LayoutUpdateStrategy` as ordinary writable dependency
properties. The adapter now uses styled properties for all four. This audit
checks their style participation and lifecycle wiring separately from API-key
presence; it does not claim identical WPF/Avalonia binding lifetimes.

| Contract | Implementation and evidence |
| --- | --- |
| Source styles | Conditional selectors and compiled style bindings import current contents. Local values override styles; clearing them restores styled sources. Deactivation removes imported contents and releases collection subscriptions. |
| Strategy styles | Current effective strategy participates in insertion. Clearing local overrides and removing selectors restores the appropriate lower-priority value. |
| Initial layout | Each manager creates its own locally assigned root with panel, document-pane group, and empty document pane. Style-supplied layouts become effective after the constructor value is cleared; layouts remain exclusive to one manager. |
| Null assignment | Both the CLR setter and property API create a fresh empty layout for null, without raising `LayoutChanging`. Ownership moves and `LayoutChanged` is raised. |
| Coercion notification | Non-null assignment raises `LayoutChanging` before the effective value changes, including same-layout reassignment. A thrown callback leaves the previous layout effective and owned. |
| Protected hook | `OnLayoutChanged` surrounds ownership transfer, container/host replacement, source migration, and `LayoutChanged`. Suppressing or throwing before the base call leaves those side effects untouched. Construction calls the hook with a null previous layout after internal services are created. |
| Package consumers | Both XML namespaces compile conditional style bindings for all four properties; consumer probes replace data contexts and collections, deactivate styles, and assign a null layout. |

The remaining direct properties were checked against upstream registration:
read-only container/model flags retain read-only properties, and native adapter
extensions and bindable wrappers for upstream CLR models retain their distinct
contracts. `LayoutFloatingWindowControl.IsMaximizedProperty` was corrected in
the separate floating audit below. Avalonia property priority, binding lifetime,
and native window behavior remain framework-specific.

## Floating control contract audit

The three upstream floating control classes were reviewed against the pinned
source, separately from the declaration inventory. The table identifies evidence
and platform boundaries; it does not claim all inherited WPF window behavior.

| Surface | Implemented contract and evidence |
| --- | --- |
| `Model`, `CanClose`, `CanHide`, `DoHide`, tool-window commands | Current virtual model, typed descendant snapshots, live command lookup, parameter forwarding, redirected-model subscriptions, and null/managerless queries are covered by `FloatingOverrideContractTests` and `FloatingWindowCommandTests`. `DirectCommandExecutionTests` verifies direct execution without a capability query, synchronous errors, and missing-context/command failures. |
| Typed caption hooks and `RootDocumentLayoutItem` | Constructor-model activation and item lookup are covered by override tests; `NativeWindowMessageTests` covers caption/menu routing. |
| `OnClosing`, `CloseInitiatedByUser` | Cancellation precedes hide hooks and preserves callback changes/failures. The new override tests supplement the recorded WPF close scenarios and owner/close-origin tests. |
| `OnClosed` | Explicit typed overrides unsubscribe after the base lifecycle succeeds. The base removes the host and collects the captured root before `Closed`; native factory notification follows even when collection removes an empty model. There is no second collection after `Closed`. `FloatingClosedHookTests` covers suppression, errors/replay, redirected models, command/binding lifetime, and observation during callbacks, supplementing the existing close/model reference tests. |
| Focus after close/hide | `FloatingFocusRestorationTests` covers 27 owner/current-model/first-focusable-target cases, callback redirection and errors, disabled/hidden targets, and the ordering after `Closed` but before typed cleanup. Only one target is attempted. Native desktop activation checks remain deferred. |
| `IsContentImmutable` | Loaded-only content coercion is covered by `FloatingControlTests` and external consumer construction. |
| `IsMaximizedProperty`, `OnStateChanged` | Writable property metadata, bindings, native state synchronization, and persisted bounds are covered by override/control/bounds tests. Avalonia reports native state changes; WPF's internal Win32 state-loop ordering is not reproduced. |
| `SetIsDragging`, `OnIsDraggingChanged` | Public state and override notifications follow native Dock drag state. Native Dock owns capture and drag execution; setting a Boolean does not perform WPF's global mouse capture. |
| `OnInitialized` | Typed subscription suppression and empty-document closure are covered by `FloatingInitializationTests`. Avalonia can initialize during base construction, so model-dependent work is deferred; WPF initialization timing is not reproduced. |
| Keyboard hooks | Per-manager enablement, routed editor handling, desktop scaling, persistence, and floating navigator input are covered by keyboard/navigator tests. |
| `FilterMessage`, resize borders, native focus | Windows caption policy and resize hit tests have Headless coverage plus the previously recorded desktop checks. WPF client-area rewriting and its activation message loop remain native Avalonia/Dock responsibilities; current-package desktop checks are deferred. |

## Direct default-command behavior

Default item and tool-window commands retain ReactiveUI execution while exposing
AvalonDock's synchronous `ICommand` contract. `Execute` does not call
`CanExecute`; item parameters are ignored and floating-window parameters are
forwarded. Original application errors return to the executing caller, including
nested calls and disposal during a callback. Detached owned commands release
their targets and remain disabled, as documented for the native lifetime.

`DirectCommandExecutionTests` covers 23 cases; `DefaultCommandActionContractTests`
covers 38 context, group-transfer, callback-order, failure, and close-all cases.
Twelve added operation cases verify that direct close/auto-hide/dock actions
bypass availability flags without changing native policies or losing cancellation
and model identity. The existing explicit-operation cases now distinguish the
native factory's guarded operations from direct compatibility command actions.

Split availability checks `CanMove`, document count, and orientation policy;
adjacent-pane movement does not require `CanMove`. Direct split/move actions
retain invalid-context/index failures and use the Dock-backed collections.
Content moves into a new pane before the group publishes it, followed by native
focus and activation. Close-all snapshots contents, checks `CanClose`, and then
uses each current command's availability or the model fallback when null.
An earlier callback can replace a later command; removing its container fails
the later lookup. `LayoutItem.Float()` remains an unused protected no-op.

Twelve external consumer probes reproduce these focus/command defects against
the preceding `32f4de07d` packages. This audit supplements the command inheritance
and binding checks; it does not establish all WPF framework behavior.

## Direct model docking and group minimum metadata

The model-operation audit adds 52 Headless cases for manager context, return
indices, cross-layout insertion, protected `InternalDock`, completion/activation
ordering, garbage collection, and existing floating-pane restoration. Explicit
model edits use public collections backed by the original native Dock objects.
The no-history fallback does not force activation. `Docked` uses the manager of
the resulting layout and precedes the final floating flag and cleanup phases.
The 36 group-minimum cases cover all five concrete positionable group kinds,
both axes, native/public property names, XML values, repeated `NaN`, validation,
reentrancy, and observer failures.

The independent package consumer adds 12 minimum-metadata and 16 model-docking
checks. Against the preceding `7d5dd7f28` packages, all 12 minimum checks and 15
of the 16 model checks reproduce defects; the remaining insertion-index case is
a passing control. These checks use the pinned source contracts, not new desktop
observations. Interactive desktop validation remains deferred.

## Root property and side lifetime contracts

The root-property batch adds 37 property, construction, traversal, XAML, XML,
replacement, and observer-failure cases, plus a native floating-close cancellation
case for `LayoutRoot.ReplaceChild`. Thirteen independent consumer probes reproduce
the earlier `532c692c1` package failures, including compiled null slots in both
XML namespaces. All 3,413 adapter and 30 rendering cases pass after the changes.

The subsequent side-lifetime audit adds 23 cases for retained parent/root
references, silent removal from root slots, cross-root assignments, shared slots,
unchanged side values when the parent is unchanged, reentrant callbacks, incoming
parent/side observer failures, native pinned ownership, and released model
lifetimes. The public slot and parent references follow the pinned source
contract; one native projection follows a current slot. Replacing a strip does
not raise parent or membership notifications for the former strip or its tools.
Incoming callback errors preserve the completed public phase and synchronize
native ownership with the resulting parent. Fourteen independent package probes
cover the root-slot and native projection contracts; all 14 reproduce failures
in the preceding `ec18570ec` packages. All 3,436 adapter and 30 rendering cases
pass. No desktop checks were added.

## Parent transition contracts

Parent transitions retain the root recorded after the last successful parent
hook and advance that record before calling the root hook. Ancestor movement
does not rewrite descendant records. Failing parent, root, and property hooks
retain their completed phases, and reentrant property observers determine the
root receiving `Updated`. `Parent` notifications do not add a `Root` property
notification. The root getter starts at the parent; `GetRoot()` includes self.

Application-defined containers can be public parents without native ownership.
Custom root interfaces reach the protected hook; the default implementation
retains the upstream concrete `LayoutRoot` cast. A side's default parent hook
requires a root and fails after assigning null or an unrooted parent. Native
pinned ownership is reconciled after failed or suppressed hooks, and a later
valid assignment restores its current root slot's projection.

The batch adds 35 parent-contract cases and four command/focus cases. Existing
floating fixtures now use `RootPanel` when they require an enclosing manager;
an unparented `LayoutRoot` does not supply that context. Sixteen external package
probes cover the source contracts: 15 reproduce failures against `176ce80ce`,
and one root-hook failure/recovery check is a passing control. These are pinned
source-contract checks; interactive desktop validation remains deferred.
The final consolidated run passes all 3,475 adapter and 30 rendering cases. One
earlier run reported manager retention in
`AnchorablePaneTitleTests.A_Retained_Closed_Title_Does_Not_Retain_Its_Model_Or_Manager`.
That case passed three isolated runs and the complete rerun without a code
change; the intermittent result is retained in the validation evidence.

## Root update notifications

The public `Updated` event is raised only by completed parent transitions,
following the pinned source's single call site. Group-tree propagation,
collection reorder/removal, and floating child removal do not add root events.
An incoming root panel or floating child reports its parent attachment once,
before the enclosing property notification. Reentrant tree observers can move
the group; only that actual parent transition reports the new root.

Native structure notifications are internal and independently refresh commands
and deferred item cleanup. This includes floating children that retain their
public parents and subclasses that suppress public property notifications.
Headless and external package probes cover the public event and native behavior.
The consolidated run passes all 3,501 adapter and 30 rendering cases, including
26 focused root-update cases. All 11 new external checks reproduce the
extra notifications in the preceding `d6694eaf9` packages.

## Tool-source root placement and pane parent notifications

When tool sources need a new pane, the replacement outer panel contains the
original panel before it is assigned to the root. Source batches reuse that
destination. A callback failure while publishing the outer panel leaves the
completed subtree intact and stops before inserting the tool. A live collection
notification reads the current manager layout again after wrapping the original
panel; an initial import keeps its supplied root throughout the operation.

Tool-pane parent hooks notify `IsDirectlyHostedInFloatingWindow` and switch the
parent's collection subscription before the inherited hook. They do not add an
`IsHostedInFloatingWindow` property event. The latter getter still reads the
current ancestry for both public and native owner changes.

Six source-placement cases and two pane-parent cases pass, together with the
existing source, parent-policy, and selection suites in focused runs. Commit
`c398082aa` passes 3,542 adapter tests, 30 rendering cases, and isolated .NET 8/10
package consumers, including these follow-up changes.

### Pane selection callback and reorder audit

Public selection now retains the pinned source's flag-before-parent ordering,
nested index clearing, unchanged-value checks, exception phases, and document/tool
collection and `ChildMoved` ordering. The coordinator records the public index;
Dock still owns content state, native selection, focus, and presentation. Internal
projection coalesces nested clears and protects public tab moves from temporary
native item-generator selection. This keeps open menus and redocked editor views
attached while preserving the public callback sequence. A rooted document's
fallback activates the child chosen before a callback, even if that callback
detaches its pane.

Native [PR #1177](https://github.com/wieslawsoltes/Dock/pull/1177) adds
`ShouldSynchronizeDockingWindowSelection` so projection can preserve those flags
while retaining native factory events and tracking. Its default behavior is
unchanged. It was implemented in a separate worktree, merged to `master`, and
integrated here by merging `master`.

The audit adds 33 Headless cases plus 16 isolated package-consumer checks. The
package checks all reproduce the defects against the earlier `470e973d2` package.
Existing tests now assert the source-defined nested notifications and differing
document/tool reorder hooks. Consolidated validation is recorded with the PR.

### Layout collection and restoration audit

Collection now validates references through their container's parent/root,
preserves saved identifiers, clears empty pane references only for visible
nonfloating content, and leaves referenced panels intact. It removes panes in
forward tree order, retains the last document pane, and runs the ordered
container-pruning phases only after a pane removal. An emptied root panel resets
to the default document layout. Tool-group collapse precedes document-group
collapse; both rescan the current tree and consume live children after callbacks.
Native document-host closure retains its deferred cancellation contract.

Pinned and hidden XML return owners now refresh native docking state before
content callbacks and collection. This preserves floating return destinations
that native initialization previously marked docked before resolving IDs.
The audit adds 28 Headless cases, a second floating-close phase case, and eleven
package checks that all reproduce earlier defects in `c398082aa`. Existing
floating tests now distinguish populated return panes from empty panes, and
unshown floating models from the deferred closure of shown hosts. Consolidated
validation is recorded with the PR. Public group child projection now preserves
the focused document/tool instead of focusing a structural container during
cleanup; all eight affected grouped outer-drop cases retain their original
focus expectations.

### Collection replacement and hidden activation

Indexed replacement publishes `Replace` and parent detach/attach hooks even for
the same instance. Incoming parent callbacks observe the new collection membership;
reattachment reconciles a parent selected by an earlier callback. Hidden and pinned
tools use their public parents during detachment. Same-instance floating-window
replacement retains its host and native registration. Callback failures retain
completed collection phases and permit a later attachment retry.
Nested count and parent callbacks can insert another child without consuming the
outer attachment or losing native collection membership.

Explicitly activating hidden tools retains the public active flag without showing
them, including calling `Hide()` on an already hidden tool. Collection replacement
does not clear that activation. Native [PR #1178](https://github.com/wieslawsoltes/Dock/pull/1178)
adds the optional `ShouldSynchronizeDockingWindowActivation` policy with an unchanged
default. It was implemented and validated in a separate worktree, merged to `master`,
and reintegrated by merging `master`. Its 2,485 native test cases pass, including
seven new policy cases and the Release leak suite. GitHub platform checks were
still running at integration; final results are recorded with the PR.
The facade adds 42 Headless cases and fourteen external consumer checks. All
fourteen reproduce defects against the preceding `e3d2916fb` packages. The
consolidated adapter run passes 3,613 cases, and all 30 rendering cases pass.

Floating-window replacement with a different model now retains indexed
`ObservableCollection` replacement and parent-hook phases while executing native
host cleanup and registration. Closing cancellation retains the outgoing model;
callback-added windows remain projected. Throwing public callbacks retain their
completed phases and support attachment/presentation retry. Native refreshes do
not attach the incoming model early during an outgoing parent callback, and
same-instance replacement does not reopen a retained closed host. Thirteen
Headless cases and four external package checks cover these contracts.
All four external checks reproduce the prior defects in `815d9fb09`. The
consolidated run passes 3,626 adapter cases and all 30 rendering cases.

### Collection callback reentrancy

Pane children, hidden tools, and floating windows now run parent/group hooks
inside the collection's internal subscriber. Nested public edits check BCL
reentrancy before changing native membership. The single internal subscriber
still permits nested edits; additional subscribers reject add/remove/replace/
move/clear atomically. Floating projection retains immediate nested edits and
single replacement notifications, including shown-host closure and callbacks
that remove, replace, or move an incoming window. A failed insertion can be
retried to complete native ownership and host presentation.

Native [PR #1182](https://github.com/wieslawsoltes/Dock/pull/1182) prevents
`AddWindow`/`InsertWindow` from initializing a registration removed by collection
or `WindowAdded` callbacks. Transfers preserve the destination owner. It passed
2,418 native tests, including 18 new callback cases, before merge to `master`
and reintegration. This merge also includes the new splitter initialization
fix from [PR #1180](https://github.com/wieslawsoltes/Dock/pull/1180), which enables
preview settings on splitters inserted into existing proportional docks, and
the 12.1.0.9 package version update.

The adapter adds 54 Headless cases and 27 package checks. Twenty-four package
checks reproduce the previous defects at `26f941518`; three allowed pane/hidden
nested additions already passed. The consolidated result is recorded with this
checkpoint's validation evidence: all 3,680 adapter cases and 30 rendering cases
pass, with no skipped cases.

### Duplicate-content drop policy

`AllowDuplicateContent=false` now participates in native into-target visibility
and drop validation. The destination document/tool pane rejects any incoming
content with the same title and identifier, using ordinal string comparison
and matching null values. The scan covers every visible descendant of dragged
panes, nested groups, and floating roots, including mixed document/tool groups.
It uses projected public metadata for native-created content. An item is not
its own duplicate during an in-pane move; ancestor/source options do not
replace the destination pane option. Side/outer/window operations and direct
model/collection edits remain available.

This uses the existing `IDockDropPolicy` extension point and a composed adapter
policy, with no native Dock change. Fifty-six new Headless cases cover identity,
null/case semantics, live metadata changes, rejection without mutation, native
execution after enabling duplicates, subtree dragging, native-created models,
and actual overlay selector visibility. Ten external package checks include
seven reproductions of missing behavior in the preceding `9b26d086c` packages.
The consolidated run passes all 3,736 adapter and 30 rendering cases, with no
skipped cases. CI consumer restoration now maps the entire `Dock.*` package
family to the freshly packed local feed, so a version bump does not depend on
unchanged Dock dependencies already having been published to NuGet.org.

## Container metadata and binding contracts

The final item-metadata review corrects null title forwarding and the supplied
callback values for titles, identifiers, selection, activation, capabilities,
and document descriptions. Icon callbacks intentionally read the current icon.
Document container visibility updates its model and pane before closure; a
cancellation or callback failure retains the requested visibility. Tool
container visibility maps false to the upstream hidden state and calls
`Hide(false)`, independently of `CanHide` and without `Hiding` cancellation.
The callback guard is released when `Hidden` fails so restoration can be retried.

`ItemMetadataCallbackTests` adds 24 Headless cases for null/empty titles, property
clearing, document/pane state inside manager and model closing callbacks,
cancellation, exceptions, restoration, and tool capability/cancellation gates.
The external consumer adds 18 checks; nine fail against the preceding
`198fe6b12` packages.

`Title` and `ContentId` retain their native-backed direct properties and opt into
the framework's compatibility metadata. Compiled binding cases cover default,
one-way, and two-way bindings across CLR, typed, untyped, indexer, and base-object
assignments; current values, clearing, native model updates, coercion, and
disposal. Equal one-way assignments retire the expression, two-way assignments
write their source, and both `ClearValue` paths remove a local binding. No
reflection or property-store inspection is introduced.
The consolidated adapter suite passes all 3,772 cases with none skipped.

## Content control callback registration

Document/tool content callbacks remove one registration from the supplied old
model and add a separate registration for each base call on the current model.
Notifications read the live public property, including after suppressed model
hooks; old publishers therefore follow current state. The registrations use
weak references and do not retain discarded controls. Tool callback registration
requires managed content; native presentation supports transient ownership.

Disabling active content first clears its pane selection, then scans current
children for the first enabled item. Callbacks during clearing can change that
choice or stop it with an exception. Document controls only change document-pane
selection; tool controls only change tool-pane selection. Public title controls
retain the previous container when their model is cleared, while native title
cleanup continues to release both values.

Thirty new Headless cases cover replay, suppression, repeated registrations,
individual removal, redirected enablement, clear-before-scan ordering, pane
kinds, exceptions/retry, late attachment, title retention, and collection of
unused controls. Twelve new package-consumer checks all fail against the prior
`37d4e8851` packages. The consolidated adapter suite passes all 3,802 cases.
One existing retained-title lifetime case failed once in the focused run; it
passed unchanged in isolation and in the complete suite.

Standalone content controls now follow the upstream callback sequence. Document
controls subscribe before resolving `Root.Manager`, so an unmanaged model raises
`NullReferenceException` while retaining its new property value, subscription,
and previous item. Tool controls clear their item without subscribing when the
model has no manager. Assigning or clearing either model leaves control
enablement unchanged until the model raises an `IsEnabled` notification. The
resolved or explicitly supplied item survives visual attachment and ownership
changes until another base model callback replaces it. Native presenters retain
initial enablement, ownership refresh, and cleanup through their separate setup.
`ContentControlCallbackTests` and `LayoutContentControlTests` cover these states,
including suppressed hooks, failed lookup, later attachment, and native reuse.

## Inherited control property contracts

Floating document, tool, and group hosts exclude themselves from the taskbar by
default, before any theme is applied. Overlay windows default to no activation,
no taskbar entry, no decorations, and a transparent surface. A replacement
control theme can override those values, and removing a style or clearing a
local value restores the defaults. Splitters retain a transparent hit surface
with an empty replacement theme. Auto-hide focus and initial visibility use
metadata defaults instead of constructor-local values, so application focus
styles remain effective. Native Dock and Avalonia base-control defaults are
unchanged.

Fifteen new Headless cases cover those defaults, style priority, clearing,
replacement themes, and base-control isolation. Fourteen package-only checks
fail against `60e39b103`; the new implementation passes all 102 focused control
cases and the complete 3,817-case adapter suite. The 30 rendering cases pass.

## Manager key-down contracts

`DockingManager.OnKeyDown(KeyEventArgs)` now implements the Shift+F10 base hook.
It reads the current `ActiveContent`, unwraps a layout model's content, and walks
visual parents to the first public document or tool content control. Its matching
manager menu opens at that control's origin, using the control's `LayoutItem` as
the data context. An application-created content control can have no item; its
menu then receives a null context. Nonvisual application objects and detached
visuals do not open a menu; a nonvisual `AvaloniaObject` retains the invalid visual
traversal error. The hook leaves the supplied key-down event's handled state alone.

Opening happens on key press. An override can replace the menu before calling
base, or suppress the base call entirely. A canceled or suppressed opening is
not retried on key release, and a successful opening remains visible through that
release. Subsequent shortcuts retain ordinary menu toggling. Editor-owned context
menus and flyouts retain Avalonia's platform shortcut route; the Apps key continues
to request the active pane's menu through native content.

Twenty-nine new Headless cases cover those contracts, including Windows-style
Shift+F10 configuration on the headless platform. Thirteen package-only checks
all fail against the preceding `c7a9b8699` package. The complete adapter suite
passes 3,846 cases; the 30 rendering cases pass separately.

## Auto-hide focus and resize contracts

The auto-hide focus base hook returns `false`, matching upstream, while
Avalonia's `IsKeyboardFocusWithin` reports actual focus independently. Two
Headless cases fail against the preceding implementation and verify focus on
the preview, within its content, and outside it.

Resize ghosts use physical coordinates relative to the auto-hide area. The
horizontal interval is 25 through the area's width; the vertical interval is
25 through its height minus `AutoHideMinHeight`. Model minima apply when the
size is committed, so the ghost can move beyond the eventual minimum content
size. Completion adds the signed ghost movement to the stored size, or to the
displayed size when the stored value is zero. In normal untransformed layout,
the maximum committed sizes are the area width on the left, width minus 25 and
splitter thickness on the right, height minus the minimum height on top, and
height minus 25 and splitter thickness on the bottom.

An unset size uses the initial minimum when opening and remains unset until
an assignment or successful resize. Later minimum changes do not enlarge the
open preview. Inactive-axis assignments also preserve that initial fallback.
Switching models or detaching the behavior restores temporary content sizing.
The scoped native settings disable remaining-space reservation and automatic
size initialization; native controls outside the adapter retain their defaults.
Right-to-left content, all edges, inline previews, interrupted drags, runtime
splitter dimensions, and native content anchoring have targeted coverage.
Dock's additional inline mode retains limits relative to the content size;
the upstream physical ghost intervals apply to overlay previews.

Sixteen package-consumer checks all fail against `9321b1af1`, covering extreme
shrink/grow movement, ghost positions, deferred commit, rendered sizes, and
stationary content edges. The native prerequisite has 36 new cases and passes
1,623 native Headless tests plus four Avalonia 11 tests.
The integrated focus/resize pass contains 61 cases, and the complete adapter suite
passes 3,876 cases. All 30 rendering cases pass separately, with none skipped.

## Framework boundaries and deferred desktop checks

The adapter implements the Avalonia migration contracts documented here. The
following differences remain explicit and are not represented as missing native
Dock features or hidden by the API inventory:

| Boundary | Contract |
| --- | --- |
| CLR and UI framework | Public namespaces are `AvalonDock.*` as requested. WPF controls, events, metadata, templates, pointer/focus arguments, and assemblies require Avalonia counterparts. This is not binary compatibility. The 106 strict inventory differences remain individually classified in `portability-review.tsv`. |
| Binding lifetime | Opt-in framework metadata implements WPF binding replacement, current-value overrides, style-priority isolation, and direct-property clearing without reflection. The unchanged ActiveContent oracle includes all 110 WPF scenarios; native Dock properties retain ordinary Avalonia policy. |
| Logical trees and native hosting | The public logical-child enumerator hooks supply real Avalonia traversal through the framework's typed logical source. Weak manager membership and base-callback suppression follow upstream. Keyboard/context-menu routes use Avalonia arguments. WPF `HwndHost.BuildWindowCore` and `DestroyWindowCore` have no replacement hooks; pinned content uses Dock's native host. |
| Graph integrity | Public collections retain duplicate occurrences and upstream Reset parent references while projecting unique native nodes and detaching removed native ownership. Cycles remain rejected. Both WPF `DependencyObject` and Avalonia `AvaloniaObject` seal object equality. Native floating-close cancellation remains effective. |
| XML/custom resources | Custom CLR XML types and compiled resource URIs use typed factory registration. Runtime assembly discovery and arbitrary WPF resource loading are absent. Custom model hooks still participate through their public virtual methods. |
| Additional floating layouts | Generic/mixed Dock groups use the additional `LayoutGroupFloatingWindow` XML element, which WPF cannot consume. Hidden/pinned contents inside a floating root, nested windows, and incompatible typed-window mutations are rejected before a file is overwritten. |
| Auto-hide layout | Content and resizers use Dock's pinned host with adapter-controlled spacing and unset-size persistence. Physical resize ghost limits follow upstream; the visual tree differs from WPF's popup grid. |
| Preview geometry | Full-content source snapshots are implemented and enabled by default. Exact projected destination layouts remain disabled because the stock clone cannot reproduce the adapter's custom grids and typed policies; the documented half/third destination indicators remain available. |
| Framework initialization and input | Avalonia initialization, capture, routed input, window decoration, and focus behavior remain platform contracts. Source-level hooks and automated tests do not establish identical OS behavior. |

Current-package interactive desktop checks are **deferred at the user's request**.
They cover broader native drag/float/redock, grouped window lifecycles, pinned
resizing/focus, navigator modality, and overlays across Windows, macOS, and Linux.
Earlier Windows 11 ARM64 checks include 37 pinned-window checks and native
message/menu, navigator, and drag scenarios. The retained `e04288110` Windows
publication is older than the current package and is not evidence for its exact
commit. Headless, rendering, package, and hosted CI results are recorded separately.

## Reproducible API inventory

`UPSTREAM-API.txt` records 951 public/protected declarations across 102 exported
types in the pinned upstream source. The Roslyn-based source audit counts 845
matching type/member keys and 106 missing declarations at this checkpoint.
All 102 exported type keys are present; signatures, inheritance, templates, and
behavior have the separate acceptance evidence and framework boundaries above.
This is a **presence audit only**, not a percentage of completed compatibility.
It includes the upstream control, theme, converter, and resource surfaces.

The separate [declaration-shape review](../../build/AvalonDockApiAudit/SIGNATURES.md)
records 184 textual signature differences and their explicit contracts, plus
the modifier and generic-constraint review. It fixed three parameter names needed
by upstream named-argument calls without changing the presence counts.

Comparison maps the requested `AvalonDock.*` namespace and canonicalizes known
`CultureInfo`, `CancelEventArgs`, XML reader/writer and IO stream/reader/writer
qualifications, and array rank spacing. This recognizes ten existing XML/IO
declarations previously reported as missing; it does not add new implementation.
No baseline entries are
removed or WPF framework types equated to Avalonia types. For example, the
navigator's preview-key and selection hooks exist with Avalonia event arguments,
but their keys remain distinct from the upstream WPF argument types.

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
