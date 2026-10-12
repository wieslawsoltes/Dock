# NuGet consumer smoke test

The update-strategy probe replaces a strategy during a source batch, verifies
that the replacement receives the after callbacks, and redirects a hidden tool
to a different pane and tab index through `Show()` using the packaged models.
It also renders native content during an insertion callback, checks that lookup
stays empty until registration, and then executes the updated native close button.

Metadata and utility probes load consumer-owned compiled image assets through
the public URI converter, resolve resource strings with a culture override,
compile resizers in both XML namespaces, and check the actual native tool tab's
live icon, tooltip, and enablement. They also verify that the public logical-child
enumerator exposes the existing item container.

The overlay probe compiles themes and target enums through both XML namespaces,
creates an overlay through the inherited native factory, checks the public area
against its existing pane, compares selector geometry with native hit testing,
and verifies that closure releases the projection and hosted control.
The outer-preview probe drags a native tool tab through a scaled layout, checks
the inherited `IDockAdornerHostProvider`, and compares the active highlight with
one third of its enclosing document group. It also resizes and moves the window,
completes the outer drop, and checks that the adorner releases its inset.

Anchor probes compile side/group/anchor controls and manager templates through
both XML namespaces, invoke external side-panel change hooks, and inspect the
actual native model hierarchy. They exercise passive programmatic previews,
pointer activation and native pinned reordering, live template/title changes,
side clear/restore, XML content identity, and visual reattachment.

Pane-control probes compile themes for both public pane types through both XML
namespaces, exercise external constructor/selection/manager hooks, and check live
model identity, passive selection, empty selection, reordering, runtime themes,
and existing content views across document floating and docking.
Headerless panes compile in both XML namespaces; their live `ShowHeader` setting
hides the tabs and document menu while retaining the content view and respecting
Dock's dynamic tab-strip visibility resource.
Sizing probes retain all-pixel pane lengths across host resizing, commit native
splitter previews only on release, apply minimums without replacing requested
lengths, and restore those dimensions from XML on both axes. Auto probes verify
content-driven sizing, minimums including chrome, nested intrinsic star sizing,
XML/content identity, and preview conversion from Auto to pixels on release.
External pane and tool-group subclasses override width/height change hooks,
verify single-child pixel propagation, and restore independent XML lengths
without invoking the hooks or overwriting an existing child's dimensions.
The external grid
probe also verifies that the packaged native create-button host does not parent
the document model into the UI.

This project intentionally uses package references only and is excluded from
the solution: the package must be built before it can restore. It checks
parent/root hooks from an external derived document and layout membership events,
selection when attaching preselected content, and an external pane's protected
visibility setter and hook with ancestor visibility propagation.
Pane checks separate selection from activation, exercise an external `ChildMoved`
override after reordering, and use the native factory to verify document-history
and enabled-tool fallback. Emptying a rooted document pane clears root activation,
including an active tool in another pane.
An external document overrides selection, activation, closing, and closed hooks.
The consumer verifies completed state during activation, native activation of an
already selected tab, cancellation, and detachment before the closed callback.
Tool titles compile in both XML namespaces, expose their existing item container,
and support external model/input overrides and the protected container setter.
The consumer checks native title presentation and a custom title's whole-pane
float through Dock's cancelable window-drag entry point.

Public document/tool tab headers compile through both XML namespaces, retain the
existing item in native docked/floating tabs, and expose externally overridden
model and pointer hooks plus the protected container setter. The consumer checks
the packaged optional native drag policy, restrictions on native and custom tab
reordering, custom-row movement, and floating one tab through canceled native
window dragging.

Public document/tool tab panels compile in both XML namespaces. External probes
override measure/arrange and the document mouse-exit hook, verify compressed tool
widths, restore application bindings after document overflow, and select a hidden
document through a compiled pane-bound row without changing activation. Native
docked/floating tabs also verify that their public panels are in use. The native
drag helper excludes noninteractive targets before both factory and collection
moves.

Content controls compile in the default and historical XML namespaces, expose
their existing model containers, and support an external model-change override
and protected container setter. The consumer checks model enablement and verifies
that native docked/floating document and tool controls display the same item view.
It also verifies that a real consumer can compile the default Avalonia XML namespace, render
Dock, apply compiled container styles, content templates and tab/auto-hide
headers, render tool and floating document titles and select their templates, click
floating caption buttons to maximize/restore and execute the current close command,
native tabs and custom manager menus to execute container commands, activate
documents through the list menu with compiled header and icon templates, verify
its public `MenuItemEx` containers and preservation of an icon when a selector
returns null while its template changes, resize
native splitters through the manager properties, observe bubbling docking/floating
notifications around native movement, inspect typed native floating controls and
their collection changes, create and explicitly show an immutable floating window,
bind unprefixed window-wide commands and invoke current item commands with parameters,
hide and restore a tool while retaining its floating host and native registration,
activate a floating document through manager content, verify its owning native
root and last-focused-document state, and clear activation without assigning it
to the primary root,
enable unprefixed keyboard movement and move a floating host with Headless arrow-key input,
style unprefixed resize borders and verify native resize roles and pointer handling,
inspect the protected close-origin property from a derived consumer control,
preserve host/model ownership during canceled cleanup and release it on retry,
resolve a container and activate, hide, and auto-hide tools through unprefixed
command converters, update converted visibility and preserve null binding targets,
open the navigator with native Ctrl+Tab input, apply unprefixed label styles,
cycle its document/tool lists, invoke its public template hook, and activate/focus
the chosen document on Control release while its scoped timeline delays presentation,
then focus it after resuming the timeline, compile unprefixed `GenericTheme`, include
its packaged resource URI, share and update a dictionary theme across native
floating and navigator windows, replace it and release the old dictionary owner,
and verify that canceling an owner's close
preserves hidden navigators while completing it releases them and permits
`OnLastWindowClose` desktop lifetime shutdown,
and restore application content
from XML using the packaged assemblies. Pack the native
control, core model, Avalonia model, Fluent theme, and proportional-panel packages too: they contain the tool-menu
lifetime, window drag input, operation observation, model-aware host creation, unique
host registration across hide/show, canceled-exit ownership preservation,
hidden-window drop/activation guards, native ownership-changing notifications,
replacement-selection, automatic-focus and tab-drag policies, model-safe create-button
presentation, and runtime splitter sizing
changes. An isolated cache
and package-source mapping ensure these local packages are tested even when the
same version exists on nuget.org or in the normal package cache.

```sh
dotnet pack src/Dock.Avalonia.AvalonDock -c Release -o artifacts/nuget
dotnet pack src/Dock.Avalonia -c Release --no-build -o artifacts/nuget
dotnet pack src/Dock.Model -c Release -o artifacts/nuget
dotnet pack src/Dock.Model.Avalonia -c Release --no-build -o artifacts/nuget
dotnet pack src/Dock.Controls.ProportionalStackPanel -c Release --no-build -o artifacts/nuget
dotnet pack src/Dock.Avalonia.Themes.Fluent -c Release --no-build -o artifacts/nuget
dock_consumer_validation=$(mktemp -d)
cat > "$dock_consumer_validation/NuGet.Config" <<EOF
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$PWD/artifacts/nuget" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="Dock.Avalonia.AvalonDock" />
      <package pattern="Dock.Avalonia" />
      <package pattern="Dock.Model" />
      <package pattern="Dock.Model.Avalonia" />
      <package pattern="Dock.Controls.ProportionalStackPanel" />
      <package pattern="Dock.Avalonia.Themes.Fluent" />
    </packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
EOF
dotnet restore tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  --configfile "$dock_consumer_validation/NuGet.Config" \
  --packages "$dock_consumer_validation/packages"
dotnet build tests/Dock.Avalonia.AvalonDock.PackageSmoke -c Release --no-restore
dotnet run --project tests/Dock.Avalonia.AvalonDock.PackageSmoke \
  -c Release -f net10.0 --no-build --no-restore
```

Override `DockCompatibilityPackageVersion` when testing a prerelease package.
Both target frameworks are compiled; the executable smoke test uses .NET 10.

The lifecycle probe derives a manager and floating window in this external
consumer assembly. It invokes the initialization/template hooks and checks
that the BCL closing hook shares Avalonia cancellation arguments and permits a
later retry. The overlay probe reads native selector registrations directly.

The destination probe exercises the packaged `IDockDropPolicy` through native
target visibility and drop execution. It rejects a document drop into a floating
tool window, accepts a tool split, verifies typed floating ownership, and saves
and restores the resulting layout through XML.

The document-split probe drops a three-tool pane on all four document edges.
It checks the packaged split policy, typed document pane/group, source tab order,
and XML restoration using only NuGet references.

The serialization probe defines an external `WriteXml` override, verifies that
the complete serializer invokes it, restores documents/tools sharing a content
identifier, and checks callback ordering and content closing-event cancellation.

The menu probe opens compiled `DropDownButton` and `DropDownControlArea` menus,
activates the existing layout item command, and reopens their nested icon bindings.
`MenuButton.axaml` uses the historical URI as its root default namespace; the
ordinary Avalonia window disambiguates `ad:DropDownButton`. It also compiles and
invokes the public menu, icon, and dropdown protected hooks from this external
consumer assembly.

The anchor probe also exercises `LayoutAutoHideWindowControl` through a
window-scoped theme: compiled public model/background/content-theme styles,
existing content identity, external manager hooks and preview replacement,
XML cleanup, reattachment, idle closing, and negative-delay coercion.
It also compiles and invokes all six `AvalonDock.Controls.Extentions` helpers
against the rendered native preview and its manager/window hierarchy.
