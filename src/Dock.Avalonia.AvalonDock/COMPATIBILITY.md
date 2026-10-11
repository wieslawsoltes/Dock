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
- Document close cancellation, tool hide/restore, hidden collection, active
  content, and title/content/capability forwarding.
- `PreviewFloat`, `Floated`, `PreviewDock`, and `Docked` bubble through the
  manager and its ancestors with the public layout content as `Source`.
  Protected notification hooks support derived managers. Model methods,
  container commands, menu pointer input, and native drag execution share
  operation scopes; internal split/group moves do not duplicate notifications.
  Native validation, tab reordering, and reuse of an existing floating pane are
  silent. Tests cover timing against live native ownership, group selection,
  grouped redocking, handled routing without cancellation, capabilities,
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
- 339 passing tests in `Dock.Avalonia.AvalonDock.Tests` as of this checkpoint.
  NuGet pack produces net8.0 and net10.0 assemblies, XML docs, and the package
  documentation with dependencies on Dock's Avalonia model, Fluent theme, and
  ReactiveUI commands.

## Open work (not release-complete)

- Complete native floating lifecycle/state synchronization, including native
  group redocking, generic panels, and grouped-document/mixed native windows. XML rejects
  native floating shapes that cannot be represented by the AvalonDock hierarchy
  before overwriting a saved file.
- Remaining manager APIs: additional update strategy invocation paths,
  templates/selectors, styles, themes, floating-control collection events,
  converters, and the control adapter surface.
- All upstream interfaces, extension methods, protected extensibility points,
  and `IXmlSerializable` methods not yet represented in the adapter.
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

## Reproducible API inventory

`UPSTREAM-API.txt` records 951 public/protected declarations across 102 exported
types in the pinned upstream source. The Roslyn-based source audit counts 425
matching type/member keys and 526 missing declarations at this checkpoint.
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
