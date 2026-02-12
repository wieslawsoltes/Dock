# BrowserTabTheme Migration Plan

## Goal
Bring `BrowserTabTheme` to feature parity with upstream Dock themes where it is intended to apply, while preserving browser-tab UX decisions and explicitly excluding unsupported layout modes.

## Scope

### In Scope
- Theme architecture parity (resource structure, token usage, control coverage for supported surfaces).
- Runtime behavior parity for supported controls (capability policy, docking interactions, overlays used by supported flows).
- Build/test parity expectations for a Dock sample theme.

### Out of Scope (Intentional)
- `MdiDocumentControl` / `MdiDocumentWindow`.
- Pinned tool surfaces (`ToolPinnedControl`, `PinnedDockControl`, `PinnedDockWindow`, pin item workflows).
- Layout modes not used by BrowserTabTheme:
  - `StackDockControl`
  - `SplitViewDockControl`
  - `GridDockControl`
  - `UniformGridDockControl`
- Any changes under `src/Dock.Avalonia.Themes.Fluent`.

## Current State Summary
- Submodule is updated to latest upstream with PR `#1047`.
- Browser theme includes major browser-specific templates and tokenized accents.
- Capability policy bindings were partially aligned in key controls.
- Structural parity with Fluent theme is incomplete: several control dictionaries and theme API hooks are missing.

## Migration Workstreams

## 1. Theme Shell Parity
- [ ] Add a Browser theme class API surface equivalent to upstream theme classes where applicable.
- [ ] Decide and document supported theme-level knobs:
  - density mode support (`Default`/`Compact`) for supported controls only.
  - optional preset switching strategy (if needed for sample UX).
- [ ] If density is supported:
  - add `DensityStyles/Compact.axaml` for BrowserTabTheme.
  - wire it through theme class resource lookup (same pattern as Fluent/Simple themes).

Acceptance:
- Browser theme can be instantiated and switched without runtime resource errors.
- Density switch (if implemented) changes expected tokenized sizing.

## 2. Resource Topology and Token Contract
- [ ] Finalize token-first `BrowserTabAccents.axaml` as canonical source.
- [ ] Keep compatibility aliases for legacy keys still used by Dock controls.
- [ ] Add missing token keys used by included Browser control templates.
- [ ] Remove duplicate hard-coded dimensions/brushes from controls where token keys exist.
- [ ] Document Browser-specific token overrides vs inherited defaults.

Acceptance:
- No unresolved resource keys at runtime.
- Most control size/color references are token-backed (not literals).

## 3. Supported Control Coverage (Template Parity)
- [ ] Audit each included Browser control template vs current Fluent counterpart.
- [ ] Port non-visual behavior deltas from Fluent into Browser templates for supported controls:
  - `DockControl`
  - `DocumentControl`
  - `DocumentTabStrip`
  - `DocumentTabStripItem`
  - `ToolControl` (if tool surfaces remain supported)
  - `ToolChromeControl` (if tool surfaces remain supported)
  - `ToolTabStrip` / `ToolTabStripItem` (if tool surfaces remain supported)
  - `DockTarget` / `GlobalDockTarget`
  - `DragPreviewControl` / `DragPreviewWindow`
  - `HostWindow` / `HostWindowTitleBar`
- [ ] Keep browser visual decisions (shape, spacing, close button styling, create button placement).

Acceptance:
- Functional parity for supported interactions with Browser visuals preserved.

## 4. Include Graph Cleanup
- [ ] Update `Styles/Controls/AllResources.axaml` to include only supported controls.
- [ ] Remove unsupported control dictionaries from Browser sample (MDI, pinned, unsupported layouts).
- [ ] Ensure no remaining references to removed dictionaries/resources.

Acceptance:
- `AllResources.axaml` is minimal and coherent with supported scope.
- Build succeeds without orphaned resources.

## 5. Docking Behavior Parity for Supported Modes
- [ ] Keep no-split document behavior via `DockProperties.ShowDockIndicatorOnly=True` + `IndicatorDockOperation=Fill`.
- [ ] Validate global dock indicators against current upstream behavior.
- [ ] Confirm capability policy behavior for:
  - drag/drop gating
  - float/close command visibility
  - docking commands in context menus

Acceptance:
- Behavior aligns with upstream capability policy semantics in supported flows.

## 6. Overlay / Command Surface Decision
- [ ] Decide whether BrowserTabTheme should support Dock selector/command overlays for current sample UX.
- [ ] If yes, port required overlay dictionaries:
  - `DockSelectorOverlay`
  - `DockCommandBarHost`
  - `OverlayLayerStyles`, `OverlayLayerDefaults`, `OverlayDataTemplates`
  - dialog/confirmation overlays required by those flows.
- [ ] If no, explicitly document and remove related unused references.

Acceptance:
- Clear supported behavior with no partially wired overlay surfaces.

## 7. Sample App Integration
- [ ] Keep BrowserTabTheme sample app wiring aligned with theme design decisions.
- [ ] Ensure `App.axaml` does not carry behavior hacks that belong in theme/control templates.
- [ ] Update sample README with supported/unsupported matrix.

Acceptance:
- Sample app wiring is minimal and theme-driven.

## 8. Test and Validation Matrix
- [ ] Build validation:
  - `dotnet build samples/BrowserTabTheme/BrowserTabTheme.csproj`
- [ ] Runtime manual checks:
  - tab render + close states
  - create button behavior
  - drag within document dock
  - drag out to float (if supported)
  - no-split document behavior
  - global docking indicators for supported flows
- [ ] Add/extend headless checks if this sample is promoted to maintained parity target.

Acceptance:
- Validation checklist passes with no runtime resource exceptions.

## Execution Order (Recommended)
1. Theme shell + token contract finalization.
2. Supported control behavior parity pass.
3. Include graph cleanup and removal of out-of-scope dictionaries.
4. Overlay/command surface decision and implementation.
5. Final verification and documentation update.

## Deliverables
- `Styles/BrowserTabTheme.axaml(.cs)` parity updates.
- `Styles/BrowserTabAccents.axaml` finalized token contract.
- `Styles/Controls/*` supported templates aligned.
- `Styles/Controls/AllResources.axaml` cleaned include graph.
- Updated `port.md` + sample README supported-feature matrix.

