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

## Verified implementation

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
- `LayoutRoot.ActiveContent` and manager active-content notifications span the
  primary and floating native roots while retaining each root's own focused
  item. Model-only activation also deactivates the preceding pane before its
  view is loaded. `LastFocusedDocument` includes tools in document panes and
  notifies both affected `IsLastFocusedDocument` properties. Activation, source
  root-specific focus clearing, window reactivation, callback redirection,
  hierarchy removal/replacement, unrelated native roots, and deferred floating
  navigator selection are covered by Headless tests.
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
  panes. `DocumentPaneMenuItemHeaderTemplate`/selector and
  `IconContentTemplate`/selector receive the public layout content; icon selectors
  also receive the owning `MenuItem`. Header fallback, selector precedence,
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
  and `DoHide` participate in native closing after the factory veto. Tests cover
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
  retries, nested closed-callback cleanup, native closing before content commands,
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
  Native floating host close requests honor factory/content cancellation and
  close capabilities. Tool groups hide when they cannot close but can hide,
  retaining their floating return pane for `Show()`.
- Auto-hide sides and groups project Dock's native pinned lists, preserving
  public group ownership through pin, preview, unpin, collection replacement,
  and reordering. Headless pointer input opens the sample's Agenda strip.
- Compiled Avalonia default and Xceed XML namespaces; headless pointer tab selection and text input.
  Pixel/star pane dimensions translate to Dock proportions using each nested
  or floating panel's actual available space. `GridSplitterWidth` and
  `GridSplitterHeight` default to six and update native splitters at runtime,
  including orientation changes. Collapsed return panes and their hidden
  splitters do not consume space. Headless tests cover star ratios, pixel sizes,
  pointer drag persistence across resizing, XML replacement, reattachment,
  independent managers, and collection of replaced panels and closed hosts.
  Native Dock retains its default splitter thickness; its proportional panel
  now refreshes splitter dimensions on thickness and orientation changes.
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
  the manager's native root and factory. Missing or
  canceled content is cleaned up; hidden floating tools retain their return panes.
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
  The compiled `Themes/Generic.axaml` dictionary separates resource keys
  from selector styles and can be included by URI from consumer XAML. Tests verify
  both XML namespaces, compiled bindings, view-scoped adapter styles without
  application-wide Dock resources, the inherited Avalonia control-theme
  property, invalid configurations, and the built-in URI. General URI loading is
  still missing; custom URIs outside the built-in bundle throw `NotSupportedException`.
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
  floating layouts remain a separate compatibility gap.
- 838 passing cases in `Dock.Avalonia.AvalonDock.Tests` as of this checkpoint.
  NuGet pack produces net8.0 and net10.0 assemblies, XML docs, and the package
  documentation with dependencies on Dock's Avalonia model, Fluent theme, and
  ReactiveUI commands.

## Open work (not release-complete)

- Complete native floating lifecycle/state synchronization, including native
  group redocking, generic panels, and grouped-document/mixed native windows. XML rejects
  native floating shapes that cannot be represented by the AvalonDock hierarchy
  before overwriting a saved file.
- Remaining manager APIs: additional update strategy invocation paths,
  templates/selectors, styles, general theme URI loading, the URI image and
  localized auto-hide header converters, localization resources, and the control
  adapter surface. Floating controls still need exact close-event ordering,
  cancellation during manager layout replacement/view unload,
  and remaining protected/platform hooks.
- All upstream interfaces, extension methods, protected extensibility points,
  and `IXmlSerializable` methods not yet represented in the adapter.
  Additional selection edge cases and protected override ordering still require
  the broader signature and behavior audit below.
- Complete auto sizing, header visibility,
  repositioning policies, icons, tooltips, and enablement
  in actual rendering and behavior. Several metadata properties currently
  round-trip without applying all upstream visual or behavioral effects.
- Full native drag/float/redock and lifecycle coverage, including native-created
  nested groups, layout detachment and leak checks, and source-driven floating
  content and changes during deserialization.
- Exact signatures, framework substitutions, inheritance, defaults, and member
  behavior must be audited independently of the presence inventory, including
  menu selector null results and header-template default coercion edge cases.
- Broader sample interaction and platform coverage for floating windows.
- Navigator desktop modality/focus validation and remaining theme/overlay integration.

## Reproducible API inventory

`UPSTREAM-API.txt` records 951 public/protected declarations across 102 exported
types in the pinned upstream source. The Roslyn-based source audit counts 635
matching type/member keys and 316 missing declarations at this checkpoint.
This is a **presence audit only**, not a percentage of completed compatibility.
It includes the upstream control, theme, converter, and resource surfaces.

Comparison maps the requested `AvalonDock.*` namespace and canonicalizes known
`CultureInfo` qualifications and array rank spacing. No baseline entries are
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
