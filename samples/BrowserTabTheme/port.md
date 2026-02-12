# BrowserTabTheme Porting Guide

This document captures the current backport state from StackWich docking theme files and the exact decisions made so the port can continue later without re-discovery.

## Scope and Intent

Goal: backport the StackWich browser-tab docking look into the upstream Dock repo as a **sample-only** implementation.

Current constraints:
- Do not modify `src/Dock.Avalonia.Themes.Fluent`.
- Keep all changes in `samples/BrowserTabTheme` plus sample registration in `Dock.slnx`.
- Preserve Dock behaviors (drag/drop/float/create tab), except where explicitly restricted for browser UX.

## Current Status

Implemented:
- New sample project: `samples/BrowserTabTheme`.
- Full control-template file set under `samples/BrowserTabTheme/Styles/Controls` (mirrors StackWich docking folder file coverage).
- Typed sample-local theme: `BrowserTabTheme` (`Styles/BrowserTabTheme.axaml`, `Styles/BrowserTabTheme.axaml.cs`).
- Accent/theme aliases with Light+Dark dictionaries in `Styles/BrowserTabAccents.axaml`.
- Browser-style document tab item visuals restored (custom close button/path states).
- macOS tabstrip spacer to avoid traffic-light overlap.
- `+` create-tab button moved to left, next to tab cluster.
- Main and host windows use custom client-area chrome settings.
- Dock target asset URI fix (`avares://Dock.Avalonia.Themes.Fluent/Assets/...`).
- No-split behavior for main document area via `DockProperties` on the document dock target host:
  - `DockProperties.ShowDockIndicatorOnly="True"`
  - `DockProperties.IndicatorDockOperation="Fill"`
  - Implemented in `Styles/Controls/DocumentControl.axaml` on `PART_DockPanel`
- Capability-policy-aware bindings synced with upstream Dock changes (`PR #1047`) in key BrowserTabTheme controls:
  - `DocumentControl.axaml`
  - `DocumentTabStripItem.axaml`
  - `ToolTabStripItem.axaml`
  - `ToolPinItemControl.axaml`
  - `ToolChromeControl.axaml`
- Token migration pass completed:
  - `Styles/BrowserTabAccents.axaml` now defines token-first theme resources (`DockSurface*`, `DockBorder*`, `DockTab*`, `DockTargetIndicatorBrush`, `DockChrome*`) with legacy `DockTheme*`/`DockApplicationAccent*` aliases for compatibility.
  - Key control templates switched to token resource consumption for color/size parity (document/tool chrome, tab items, dock targets).

Adjusted sample behavior:
- Document-only layout (tools removed by request).
- Startup creates multiple documents and supports create-new-document.

## Key Files

Sample entry:
- `samples/BrowserTabTheme/Program.cs`
- `samples/BrowserTabTheme/App.axaml`
- `samples/BrowserTabTheme/App.axaml.cs`
- `samples/BrowserTabTheme/MainWindow.axaml`

Theme composition:
- `samples/BrowserTabTheme/Styles/BrowserTabTheme.axaml`
- `samples/BrowserTabTheme/Styles/BrowserTabAccents.axaml`
- `samples/BrowserTabTheme/Styles/Controls/AllResources.axaml`

Most relevant control templates:
- `samples/BrowserTabTheme/Styles/Controls/DocumentTabStrip.axaml`
- `samples/BrowserTabTheme/Styles/Controls/DocumentTabStripItem.axaml`
- `samples/BrowserTabTheme/Styles/Controls/DockTarget.axaml`
- `samples/BrowserTabTheme/Styles/Controls/HostWindow.axaml`
- `samples/BrowserTabTheme/Styles/Controls/HostWindowTitleBar.axaml`
- `samples/BrowserTabTheme/Styles/Controls/CaptionButtons.axaml`

## Resource/Alias Strategy (Hybrid)

Primary Dock keys are used where practical, with `sw*` aliases to reduce churn during template transfer.

Alias dictionary lives in:
- `samples/BrowserTabTheme/Styles/BrowserTabAccents.axaml`

Important note:
- Aliases that reference other brush resources must use `StaticResource` alias entries, not `SolidColorBrush Color="{DynamicResource SomeBrush}"`, to avoid runtime type-cast errors.

## Behavior Decisions Captured

1. Document-only UX
- No tool docks in sample layout.
- Focus is browser-style document tabs.

2. No side-splitting in main tabs
- Applied indicator-only fill docking on the document dock target host (`PART_DockPanel`).
- This is preferred over a contextual `DockTarget.Theme` selector because current Dock adorner hosting can place `DockTarget` outside `DocumentControl` ancestry.

3. Chrome behavior
- Host window and main window use client-area chrome settings aligned with StackWich intent.
- macOS left spacing added so tabs do not overlap window controls.

4. Drag-float stability
- `DockControl.InitializeFactory` must remain `true` so host-window locators are initialized; otherwise drag-out can remove tabs without opening a window.

5. Capability policy compatibility
- BrowserTabTheme keeps custom visuals but now uses `DockCapabilityConverters.*` multi-bindings for `CanDrag/CanDrop/CanClose/CanFloat/CanPin/CanDockAsDocument` so behavior tracks Dock capability policy updates.

## Known Gaps / Next Work

1. Exact visual parity for `DocumentTabStrip` container
- Current template is close but still not full parity with Sandwich top-strip composition.
- Remaining parity work is around precise structure/spacing/metrics beyond current left spacer and left `+` button.

2. Optional simplification pass
- The sample currently includes all transferred control files for continuity.
- Could later trim unused templates for a smaller maintenance surface.

3. Optional extraction path
- If this matures beyond sample, create dedicated theme package under `src/` (out of current scope).

## Porting Workflow (Continue Later)

When continuing:
1. Start by diffing the specific SW source template against the sample template.
2. Preserve Dock model/control contracts and remove SW-only namespaces/types/behaviors.
3. Map colors to Dock keys first; add aliases only when necessary.
4. Rebuild and run drag/drop + float checks after each template group.
5. Keep no-split behavior on main docs unless explicitly changing product behavior.

Useful diffs:
- `Visuals/SW.Views/Themes/Docking/*.axaml` vs `Dock/samples/BrowserTabTheme/Styles/Controls/*.axaml`

## Validation Checklist

Run:

```bash
dotnet build /Volumes/SSD/repos/StackWich/Dock/samples/BrowserTabTheme/BrowserTabTheme.csproj
dotnet run --project /Volumes/SSD/repos/StackWich/Dock/samples/BrowserTabTheme/BrowserTabTheme.csproj
```

Verify manually:
- Tabs render with browser style and close-button states.
- On macOS, tabs do not overlap traffic-light controls.
- `+` tab button appears on left beside tabs.
- Dragging tab within dock works.
- Dragging tab out opens floating window.
- Main document tabs cannot side-split.
- No runtime resource exceptions in logs.

## Out-of-Scope Reminders

- No modifications in `src/Dock.Avalonia.Themes.Fluent` for this sample port.
- No SW app-shell features (sidebar/banner/premium/feedback integrations).
