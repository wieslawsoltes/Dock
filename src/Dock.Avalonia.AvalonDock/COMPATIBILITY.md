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

The public CLR namespaces intentionally use `AvalonDock.*`, dropping the
upstream `Xceed.Wpf` prefix. Source consumers update their using directives;
the API inventory applies this explicit namespace mapping. The default
Avalonia XML namespace and the Xceed XML namespace alias are both supported.

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
| Selection distinct from activation | Factory focus and replacement-selection hooks; Avalonia-model `OwnerChanging` | Public selection notifications, activation history, and parent/root projection |
| Explicit model close and auto-hide operations | Protected close/pin core operations retaining native cancellation and cleanup | AvalonDock command-versus-model capability semantics and return history |
| Model-aware floating hosts and canceled closure | `IDockWindowHostFactory`, `IHostWindowCloseState`, and `IDockWindowClosePolicy` | Typed public hosts and public manager registration |
| Close a host while retaining its layout | Desktop and managed host policy implementations; managed reentrancy and replacement-host handling | Deferred WPF ordering and public collection semantics |
| Custom docking overlays | Scoped window factories/themes, enclosing preview hosts, selector registrations, `IDockTargetPresentation` and geometry | AvalonDock named window parts, preview path, selector mapping, and public snapshots |
| Pixel/auto/star sizing and resize previews | Custom splitter host, native tracker/input, preview properties, pinned minima and custom thumb support | Public grid lengths, normalization, and size notifications |
| Stable presentation and cleanup | Deferred root presentation, create-button coexistence, hidden-window filtering and tool-menu lifetime fixes | Public templates, layout items, themes, and retained-model subscriptions |

The [native extension guide](https://github.com/wieslawsoltes/Dock/blob/master/docfx/articles/dock-extension-policies.md)
documents contract defaults, usage, architecture, and regression coverage.
Validation is performed in a native-only worktree before reintegration, including
all model implementations and Avalonia 11 linked controls.

The remaining API inventory and behavior work is still open. Framework-specific
event arguments, logical trees, Win32 hooks, general resource-URI loading, custom
XML type reconstruction, and public compatibility callbacks belong to the
Avalonia/framework or facade boundary. A declaration-only mismatch is not evidence
of a missing native docking feature. Deferred floating cleanup uses the existing
virtual factory and host contracts; its public registration timing belongs to the
facade. Any additional native capability required by the remaining work
must follow the same separate-PR, merge-to-`master`, then reintegrate workflow.

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
pixels. Package and desktop validation of this source checkpoint remain open.

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
AvalonDock XML codec remains separate. None of these changes implements the
remaining manager callbacks, platform behavior, or automatic custom CLR type discovery.


## Verified implementation

- Eight deserialization reentrancy cases cover actual nested XML restores using
  the same or separate serializers, inner callback failures, replacing both
  sources during recovery, and restoration from `LayoutChanged`. Source
  subscriptions resume after failures, retired roots release their manager, and
  committed callback side effects remain observable. The focused serialization
  suite passes all 97 cases.
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
  deliver once with document precedence, and replacing either property preserves
  the other subscription. The portable manager type is
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
  and content `IsSelected`. Changing/changed notifications bracket the public
  selection and native flags; changed observers can immediately redirect selection
  or explicitly activate another item. Removing selected rooted documents uses
  activation history with stable child-order ties, including disabled content.
  Detached document panes and tool panes choose the first enabled child. Emptying
  a rooted document pane clears root activation, even with another active tool.
  Child moves preserve selection, report its new index, and invoke protected
  `ChildMoved` overrides; identical indices are silent. Tool collection changes
  notify aggregate capabilities, which are true for empty panes. Native factory
  selection/focus policies retain ordinary Dock defaults. Native collection
  repair waits until item generators finish; the queued fallback uses weak
  ownership. Tests cover public/native removals, derived callbacks, reentrancy,
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
  the shared WPF probe. Its 280 rows cover 110 scenarios: 102 match WPF exactly;
  eight deliberately retain Avalonia binding/style lifetime semantics. The latter
  use a separate, clearly named Avalonia fixture and test, with the common stages
  still compared against WPF. They are not counted as WPF parity. The package
  consumer runs the same operations. History notifications preserve WPF's reentrant
  order, including the outer transition's history and independent item flags.
  Assigning over one-way/default bindings and `ClearValue` retain the binding in
  Avalonia; WPF removes it. Removing a style after a current-value override can
  retain that value in Avalonia. Applications can explicitly dispose the binding
  or clear the override. Private binding-mode inspection is intentionally excluded
  to keep this implementation reflection-free; exact framework parity remains open.
- `PreviewFloat`, `Floated`, `PreviewDock`, and `Docked` bubble through the
  manager and its ancestors with the public layout content as `Source`.
  Protected notification hooks support derived managers. Model methods,
  container commands, menu pointer input, and native drag execution share
  operation scopes; internal split/group moves do not duplicate notifications.
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
  Default commands retain native policy/last-child checks and refresh on policy
  replacement. `Float` restores a previous floating pane with `CanFloat=false`,
  activates even after a rejected new float, and can refloat existing floating
  content while retaining its original docked return pane. Headless and external
  package-consumer tests exercise these differences from UI command eligibility.
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
  Model replacement updates subscriptions and initial/runtime enablement;
  disabling active content selects the first enabled sibling or clears selection.
  Protected model/container hooks and actual focus/left/right pointer routes are
  covered, including derived themes, nested managers, delayed navigator focus,
  and the floating-host focus exception. The keyboard hooks use Avalonia's
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
  Custom title input activates the tool, floats its pane or auto-hidden tool
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
  panes and tab indices. Docked tools can reuse their former floating windows;
  both return locations survive XML. Protected docking fallbacks invoke layout
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
  explicit codecs. Standalone readers consume one element, preserve existing
  native model identity and application content, and restore tab selection
  before a factory is attached. Root reads replace the hierarchy while retaining
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
  Assigning a return container raises only its changed notification, then initializes
  an unset pane ID using the reference left by application callbacks. Identifier
  metadata is quiet; changed return indices raise only changed notifications.
- Content floating dimensions initially use zero, allowing Dock to choose a valid
  native size when a window is created. Titles and tool-pane names initially use
  null. Equal title assignments are quiet, and tooltip/icon changes use reference
  identity. Changed-only content metadata, pane maximization, and header flags
  use upstream notification contracts;
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
  unchanged. The WPF `IEnumerator LogicalChildren` override is not provided:
  Avalonia uses a different, nonvirtual logical-child collection. Grouped/mixed
  layouts use the additional native-backed floating model described below.
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
  manager width/height settings and tool minima update the displayed host,
  every edge and right-to-left content supports preview/commit, and interrupted
  drags cancel. Native inline previews also resize. Size assignments clamp to
  the current minimum, preserve native bounds, and raise changing/changed events;
  negative minima are rejected. Headless tests cover input, live settings,
  cancellation, public model notifications, and retained-model lifetime.
  Ordinary native splitter defaults remain unchanged.
- `DockingManager.LogicalChildrenPublic` exposes the current Avalonia logical
  collection. Floating `OnStateChanged` overrides synchronize maximization when
  they call the base hook.
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
  style bindings remain available. The external WPF `SetValue` input is adapted
  with explicit binding disposal because inherited Avalonia `SetValue` preserves
  bindings. The reproducible reference probe is in `build/AvalonDock.WpfTemplateOracle`.
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

## Open work (not release-complete)

- Windows 11 ARM64 native pinned-window validation passes 37 checks covering
  editor attachment, native pointer resizing and release, and editor focus on
  all four edges. Broader platform coverage remains open. WPF `HwndHost`/Win32
  hooks have no direct Avalonia equivalent.
- Validate the broader native floating lifecycle and group redocking on desktop
  platforms. Grouped documents and generic/mixed panels now use the additional
  `LayoutGroupFloatingWindow` model, host, and XML element; whole-root floating
  retains a replacement main panel and return panes. That XML extension is not
  understood by upstream WPF. Floating roots with hidden/pinned contents or nested
  windows are still rejected before overwriting a saved file.
- Remaining manager API audit: templates/selectors, styles,
  and the control adapter surface.
  Floating controls still need the broader protected/platform hook audit and
  desktop input/focus validation beyond the recorded close lifecycle scenarios.
  The Windows 11 ARM64 native-message/menu checks supplement Headless policy
  tests; they do not establish full platform or floating-window parity.
- Complete the semantic audit of interfaces, extension methods, protected hooks,
  and inherited XML methods. The individual declaration review maps the existing
  counterparts; it does not establish full signature or behavior equivalence.
  Additional selection edge cases and protected override ordering remain open.
- The direct public pane/splitter Grid structure passes consolidated Headless
  validation of sizing, native drag, floating hosts, themes and lifetime. Package
  and desktop validation of this implementation remain open.
- Finish the rendering and behavior audit of repositioning policies and remaining
  metadata edge cases. Tab icons, tooltips, enablement, and document header
  visibility and public auto-hide resizers are implemented. Auto-hide content
  and resizers remain composed within Dock's native pinned host; its visual
  tree and 50-unit remaining-content constraint differ from WPF's popup grid.
- Full native drag/float/redock and lifecycle coverage, including generic and
  mixed native-created groups, layout detachment and leak checks, and source-driven floating
  content and changes during deserialization.
- Complete the remaining default, callback, inheritance-behavior and framework
  edge cases independently of the presence inventory. The declaration-shape
  review covers named arguments, modifiers, constraints and differing base lists;
  it does not establish their runtime behavior.
- Broader sample interaction and platform coverage for floating windows.
- Broader navigator modality/focus coverage on macOS and Linux, and remaining
  theme/overlay integration. Windows 11 ARM64 modality, native Ctrl release,
  owner input restoration, and editor focus have desktop validation.
- Broader desktop validation of the new named overlay presentation remains open.

## Reproducible API inventory

`UPSTREAM-API.txt` records 951 public/protected declarations across 102 exported
types in the pinned upstream source. The Roslyn-based source audit counts 820
matching type/member keys and 131 missing declarations at this checkpoint.
All 102 exported type keys are present; member signatures, inheritance, templates,
and behavior still require the separate checks below.
This is a **presence audit only**, not a percentage of completed compatibility.
It includes the upstream control, theme, converter, and resource surfaces.

The separate [declaration-shape review](../../build/AvalonDockApiAudit/SIGNATURES.md)
records 186 textual signature differences and their explicit contracts, plus
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
