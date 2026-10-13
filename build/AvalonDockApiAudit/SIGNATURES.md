# Declaration-shape review

The pinned inventory contains 951 declarations; 838 now have matching presence
keys. The signature review covered the preceding 820 matching keys, then inspected
accessibility, virtual/static modifiers, generic constraints, and differing
base/interface declarations. The two newly declared document/tool `ReadXml`
overrides match the upstream public override signature after BCL qualification
normalization and add no textual signature differences. Nine newly declared
`WriteXml` overrides and seven group `ReadXml` overrides also match the upstream
public override signatures. This is a declaration review, not evidence that every
default, callback, visual tree, or operation is behaviorally equivalent.

The review found three source compatibility defects hidden by a matching key:
`ILayoutUpdateStrategy.BeforeInsertDocument` must name its document argument
`documentToShow`, `AfterInsertDocument` must use `documentShown`, and the
`LayoutAnchorablePaneGroup` constructor must use `firstChild`. The adapter now
preserves these names. `UpdateStrategyTests` compiles calls using all three
upstream named arguments and checks their supplied values.

A separate external-consumer check found that `LayoutGroupBase` was missing the
protected parameterless constructor supplied implicitly by upstream. The adapter
now provides that constructor with a native Dock container. Because the upstream
syntax inventory contains no explicit constructor, the presence-key count cannot
identify this defect. External compilation and native ownership/notification tests
cover it. The upstream constructors on `LayoutElement`, `LayoutContent`, and
`LayoutGroup<T>` are explicitly internal; those are not exposed for external use.

The remaining 186 textual signature differences are recorded individually in
[`signature-review.tsv`](signature-review.tsv). The comparison normalizes the
requested CLR namespace mapping, known BCL qualification, whitespace, nullable
annotations, and the `partial` implementation modifier. It does not silently
equate WPF and Avalonia framework types.

The floating-control behavior audit also corrected `IsMaximizedProperty` to a
writable `StyledProperty<bool>`. Its CLR getter remains read-only, but the upstream
dependency property permits assignment, styles, and bindings. A read-only direct
property rejected those supported uses. The separate `IsDraggingProperty` remains
read-only, corresponding to upstream's dependency-property key.

The manager property audit likewise corrected `LayoutProperty`,
`DocumentsSourceProperty`, `AnchorablesSourceProperty`, and
`LayoutUpdateStrategyProperty` to styled properties. Upstream registers ordinary
writable dependency properties; conditional styles must participate in value
priority and release their values when deactivated. Compiled style bindings and
source/layout replacement are exercised in the adapter and package consumers.

| Difference | Declaration contract |
| --- | --- |
| Dependency properties and routed events | Typed Avalonia styled/direct properties and routed events replace WPF metadata objects. Defaults, binding behavior, and callback order need their separate behavioral checks. |
| Templates, styles, images, brushes and event delegates | Avalonia interfaces, control themes and routed-event delegates are explicit framework counterparts. |
| Non-public setters absent from getter-only properties | The public getter remains; private/internal setters were not callable by consumers. |
| Additional public setters | `PreviousContainer` and `PreviousContainerId` expose the existing protected setter contract more broadly. `AllowMovingFloatingWindowWithKeyboard` permits compiled Avalonia XAML assignments. |
| Notification arguments | The existing string arguments also accept the optional `CallerMemberName` form. Explicit upstream calls remain valid. |
| Qualified BCL types and the `CompatibilityTheme` alias | These resolve to the same BCL types or `AvalonDock.Themes.Theme`. |
| Hexadecimal enum spellings | The five `AnchorableShowStrategy` values are unchanged. |

The 28 differing base lists fall into these groups:

- WPF `Control`, `FrameworkElement`, `DependencyObject`, `Window`, and `HwndHost`
  require Avalonia control/object/host counterparts. `LayoutGridResizerControl`
  still derives from Avalonia `Thumb` through the native splitter. Native pin and
  floating hosts similarly retain their Avalonia control/window ancestry.
- Layout model classes inherit `AvaloniaObject`, whose `Equals` and `GetHashCode`
  overrides are sealed. Unlike upstream CLR layout models, application subclasses
  cannot override equality; collection lookup and side traversal use object
  identity. Custom `ILayoutElement` implementations remain usable by traversal
  helpers, but actual child collections require Dock-backed model wrappers.
- Public layout interfaces remain reachable through adapter base classes.
  `ILayoutContainer` comes through `ILayoutGroup`; visibility comes through
  `ILayoutPane`. Upstream positionable, initial/previous-container, pane-XML and
  overlay-host helper interfaces are internal implementation contracts.
- The adapter adds internal presentation interfaces and `IDisposable` on the
  bitmap converter. WPF's `IWeakEventListener` is framework-specific; source
  subscriptions use the adapter's weak-event services rather than that interface.

All shared generic class and method constraints agree except `DropArea<T>`:
Avalonia `Control` replaces WPF `FrameworkElement`. The modifier review found
14 WPF framework overrides exposed as virtual compatibility hooks,
five `new` declarations needed to hide different Avalonia base members, and a
virtual default for the upstream abstract `LayoutGroup<T>.GetVisibility` hook.
Existing subclasses can still override that hook; the adapter additionally
allows its inherited default. Hook routing and ordering are validated separately.

The 113 keys without exact source matches remain in
[`portability-review.tsv`](portability-review.tsv). Neither review closes the
remaining semantic and platform work in the compatibility checklist.
