# Dock Theme Token Migration

This guide helps migrate from older Dock token usage (legacy theme keys and hardcoded metrics) to the current semantic token model.

## Why migrate

Semantic tokens provide:

- stable customization points across Fluent and Simple,
- better compatibility with IDE presets,
- density support (`Normal`/`Compact`) without copying templates.

## Migration strategy

1. Keep existing themes (`DockFluentTheme` or `DockSimpleTheme`).
2. Move custom overrides from legacy keys to semantic keys.
3. Replace hardcoded metric overrides with density tokens.
4. Use `DensityStyle="Compact"` when compact layout is needed.

## Legacy to semantic token mapping

| Legacy key | Replace with | Notes |
|---|---|---|
| `DockThemeBackgroundBrush` | `DockSurfaceWorkbenchBrush`, `DockSurfaceSidebarBrush`, `DockSurfacePanelBrush`, `DockTabActiveBackgroundBrush` | Split by usage intent. |
| `DockThemeControlBackgroundBrush` | `DockSurfaceEditorBrush`, `DockTabHoverBackgroundBrush`, `DockChromeButtonHoverBackgroundBrush` | Separate editor and hover/chrome concerns. |
| `DockThemeBorderLowBrush` | `DockBorderSubtleBrush`, `DockSeparatorBrush`, `DockSplitterIdleBrush` | Use `DockBorderStrongBrush` for stronger borders when needed. |
| `DockThemeAccentBrush` | `DockSurfaceHeaderActiveBrush` | Active header/chrome background token. |
| `DockThemeForegroundBrush` | `DockTabForegroundBrush` | Base tab/chrome foreground role. |
| `DockApplicationAccentBrushLow` | `DockTabActiveBackgroundBrush`, `DockTabActiveIndicatorBrush`, `DockSplitterHoverBrush` | Token choice depends on template area. |
| `DockApplicationAccentBrushMed` | `DockTabHoverBackgroundBrush`, `DockTabSelectedForegroundBrush`, `DockSplitterDragBrush` | Separate hover, selected text, and drag indicators. |
| `DockApplicationAccentBrushHigh` | `DockTabCloseHoverBackgroundBrush`, `DockChromeButtonDangerHoverBrush` | Close/danger hover states. |
| `DockApplicationAccentForegroundBrush` | `DockTabActiveForegroundBrush` | Active text/icon foreground. |
| `DockApplicationAccentBrushIndicator` | `DockTargetIndicatorBrush` | Dock-target overlay indicator. |
| `DockToolChromeIconBrush` | `DockChromeButtonForegroundBrush` | Tool/MDI chrome icon foreground. |

## Hardcoded metric migration

Older template customizations often hardcoded values like `Width="14"`, `Height="24"`, `Padding="4,0,4,0"`.

Use tokens instead:

| Hardcoded usage | Semantic metric token |
|---|---|
| Document close button size | `DockCloseButtonSize` |
| Create document button size | `DockCreateButtonWidth`, `DockCreateButtonHeight` |
| Document tab min height | `DockTabItemMinHeight` |
| Tool tab min height | `DockToolTabItemMinHeight` |
| Document tab padding | `DockDocumentTabItemPadding` |
| Tool tab padding | `DockToolTabItemPadding`, `DockToolTabItemSelectedPadding` |
| Tool/MDI chrome button size | `DockChromeButtonWidth`, `DockChromeButtonHeight` |
| Tool/MDI chrome button spacing | `DockChromeButtonPadding`, `DockChromeButtonMargin` |
| MDI title icon sizing | `DockMdiTitleIconSize` |
| MDI drag header padding | `DockMdiHeaderDragPadding` |
| Header text padding | `DockHeaderContentPadding` |
| Modified marker spacing | `DockModifiedIndicatorMargin` |
| Tab content spacing/margins | `DockTabContentSpacing`, `DockTabContentMargin` |
| Create icon margin | `DockCreateButtonIconMargin` |
| Tool chrome title/header spacing | `DockToolChromeHeaderMargin`, `DockToolChromeTitleMargin`, `DockToolChromeMenuIconMargin` |
| Tool divider thickness | `DockToolChromeDividerThickness` |
| Shared chrome grip visuals | `DockChromeGripHeight`, `DockChromeGripMargin`, `DockChromeGripBrush` |
| MDI header/buttons layout spacing | `DockMdiHeaderColumnSpacing`, `DockMdiButtonStripSpacing`, `DockMdiButtonStripMargin` |
| MDI resize handle hit targets | `DockMdiResizeEdgeThickness`, `DockMdiResizeCornerSize` |
| Dock target selector sizing | `DockTargetSelectorSize`, `DockTargetSelectorGridMaxSize` |
| Selector overlay sizing/spacing/badges | `DockSelectorOverlay*` token family |
| Command bar layout spacing | `DockCommandBarPadding`, `DockCommandBarSpacing` |
| Drag preview layout sizing | `DockDragPreviewCornerRadius`, `DockDragPreviewHeaderPadding`, `DockDragPreviewHeaderSpacing`, `DockDragPreviewStatusSpacing`, `DockDragPreviewStatusIconSize` |
| Host title tracker strip height | `DockHostTitleBarMouseTrackerHeight` |
| Busy overlay card and progress sizing | `DockOverlayReloadButtonMargin`, `DockOverlayCardCornerRadius`, `DockOverlayCardPadding`, `DockOverlayCardSpacing`, `DockOverlayMessageFontSize`, `DockOverlayProgressWidth`, `DockOverlayProgressHeight` |
| Dialog shell sizing | `DockDialogCornerRadius`, `DockDialogPadding`, `DockDialogMinWidth`, `DockDialogMaxWidth`, `DockDialogSpacing`, `DockDialogTitleFontSize`, `DockDialogCloseButtonSize` |
| Confirmation dialog sizing | `DockConfirmationDialogPadding`, `DockConfirmationDialogMaxWidth`, `DockConfirmationDialogStackSpacing`, `DockConfirmationDialogActionsSpacing` |

## Example migration snippet

Before:

```xaml
<SolidColorBrush x:Key="DockThemeBorderLowBrush" Color="#FF3E3E42" />
<SolidColorBrush x:Key="DockApplicationAccentBrushLow" Color="#FF0E639C" />
```

After:

```xaml
<SolidColorBrush x:Key="DockBorderSubtleBrush" Color="#FF3E3E42" />
<SolidColorBrush x:Key="DockSeparatorBrush" Color="#FF2A2D2E" />
<SolidColorBrush x:Key="DockTabActiveIndicatorBrush" Color="#FF3794FF" />
<SolidColorBrush x:Key="DockTargetIndicatorBrush" Color="#663794FF" />
```

## Density migration

Before:

```xaml
<dockFluent:DockFluentTheme />
```

After:

```xaml
<dockFluent:DockFluentTheme DensityStyle="Compact" />
```

Use token overrides only for exceptions; prefer density presets for global compact/normal scaling.

## Validation checklist

1. Active tab indicator, hover, and selected states still look correct.
2. Tool chrome buttons keep expected hover/close behavior.
3. Dock selector/target overlays retain intended indicator contrast.
4. Compact mode updates tab and chrome sizes consistently.

## Related docs

- [Dock Theme Design Tokens](dock-theme-design-tokens.md)
- [Dock Fluent Theme](dock-theme-fluent.md)
- [Dock Simple Theme](dock-theme-simple.md)
