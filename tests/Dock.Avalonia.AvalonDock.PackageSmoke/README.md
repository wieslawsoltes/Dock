# NuGet consumer smoke test

Content-state probes compare 68 identifier and 88 activation-hook scenarios with
the original WPF observations. They cover named-control identifiers, null/empty
values, equal replacement objects, container inputs, bound identifiers, nested
activation, callback suppression and exceptions, and retained activation history.
Both consumer frameworks compile the shared public-operation probes.

Minimum-size probes check both axes for public/native notification names,
repeated `NaN`, unchanged negative XML values, argument errors, and reentrant
assignments. Model docking probes check missing manager context, protected
fallback overrides, insertion indices, return panes in another layout, callback
ordering, and retained return-location metadata. These use the packed assemblies
through public APIs and external subclasses.

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

The localization probe loads all 13 packaged satellite assemblies and checks
the complete 21-key resource set in each. It verifies typed resource properties,
parent-culture and English fallback, and `CurrentUICulture` lookup when the
resource culture override is null.

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
dotnet pack src/Dock.Controls.DeferredContentControl -c Release --no-build -o artifacts/nuget
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
      <package pattern="Dock.Controls.DeferredContentControl" />
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

The theme lifetime probe closes a floating document while its previous resource
provider is being removed and while a custom theme resolves the compiled generic
URI. It checks that replacement and later themes leave the closed window detached.

The floating-model probe derives document/tool models in this external assembly,
suppresses child/visibility/all property notifications, and checks immediate and
deferred host state. It also verifies retained public parents and native adoption
of removed documents and tool root panels through the packaged Dock factory.

## Floating-model reference checks

The consumer also compiles the shared public-operation probe from
`build/AvalonDock.WpfTemplateOracle` and compares all 150 floating notification
scenarios with the recorded WPF results. This covers defaults, callback state,
root changes, visibility, empty XML, and notification overrides in the packaged
assembly. Both target frameworks compile the probe; the runtime check uses .NET 10.

The active-content probe checks 102 WPF scenarios and eight explicitly documented
Avalonia binding/style lifetime differences using the shared public operations.
It covers callback state, source update triggers, alias lookup, empty and duplicate
content, layout replacement, and reentrant activation/history. The two fixture
files keep the WPF reference and the Avalonia-specific expectations separate.

The external consumer also restores registered custom XML document types, loads
a compiled dictionary through `ThemeResources`, and floats, serializes, restores,
and redocks a mixed native panel through `LayoutGroupFloatingWindow`.

The source callback probe invokes the protected document/tool hooks from this
external assembly. It verifies callback arguments after reentrant replacement,
separate delivery for shared collections, eleven repeated attachments, and
removal of one registration per detachment. Both .NET 8 and .NET 10 execute
these checks against the restored package.

The container-style probe checks retained themes after clearing both manager
providers, a selector's explicit null result, cleared default commands during
selection, suppressed protected callbacks, and replay using current manager
settings for both document and tool containers.

The root-subscription probe verifies independent registrations from repeated
protected style calls, notifications from retained old roots, exclusion of
foreign content, and removal of one registration when returning to a root.

The layout-callback probe derives an external manager, redirects its layout
before the base hook, replays supplied roots (including a foreign-owned and null
root), distinguishes initial source imports from live updates, and checks
independent root property registrations against the current panel presentation.

The template-callback probe checks callback-before-presenter delivery, retained
provider settings after failure, independent template/selector updates, current
settings for new views, reentrant child identity, and replayed selector arguments
with current document-menu fallback. It reproduces the premature-delivery defect
against the preceding package and passes only with the corrected adapter.

The layout-interface probe traverses an external `ILayoutElement` without reading
its `Root` property, verifies the parent-search cast contract, and checks group
replacement bounds, invalid child types, removal ownership, selection, and
collection/tree/count callback ordering through the public interfaces.

The group-XML probe verifies silent metadata restoration, mutable pane names
after native styling, accepted enum spellings, stored minimum dimensions, empty
group visibility, and retention of earlier values after a later attribute error.

The content-XML probe verifies derived/base setter order, immediate selection,
focus and floating metadata without activation, floating-flag persistence, partial
state and reader position after an attribute error, and external `ReadXml`
overrides with child elements. Registered content callbacks retain stored focus
flags and activation timestamps without assigning root focus. Both .NET 8 and
.NET 10 execute these checks against the restored package.

The numeric/selection XML probe checks CLR infinity spellings, current-culture
return-index round trips, deferred tool-pane fallback selection, immediate
explicit selection, and selection recovery after failed and successful reads.

The streaming XML probe checks live attribute positions, partial root restoration,
successive floating-window children, nested deserialization errors, and external
read hooks with the original depth, line information, and following siblings.

The XML write probe checks attribute omission and boolean spellings, tool/base
metadata order, whitespace-only strings, unchanged unreferenced pane identifiers,
and base write behavior on an external group with additional layout interfaces.

The read-inheritance probe checks that extra interfaces do not add metadata to
an inherited XML reader, both directly and through a registered custom factory.
The base-group probe derives `LayoutGroupBase` in this external assembly and
checks native ownership, root identity, and protected collection/tree hooks.

The writer-context probe verifies nested serialization errors, caller namespaces
and XML context, writer/model callback order, empty child namespaces, and flush
delivery at the child, pane, and root boundaries. It also checks retained writer
lifetime and stream output without a byte order mark.

The root/visibility probe checks root orientation spelling/fallback, recognized
child filtering, model interface membership, both concrete visibility passes,
and class visibility dispatch when a custom group adds an interface.
