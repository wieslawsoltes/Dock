# Dock Theme Semantic Tokens

Dock now exposes a semantic token contract on top of existing Fluent/Simple resources.

Use these tokens to style Dock surfaces/chrome without copying control templates.

For the full detailed reference, see [Dock Theme Design Tokens](dock-theme-design-tokens.md).  
For migration from legacy keys, see [Dock Theme Token Migration](dock-theme-token-migration.md).

## Token contract

Both built-in themes expose the same keys:

| Group | Keys |
|---|---|
| Surfaces | `DockSurfaceWorkbenchBrush`, `DockSurfaceSidebarBrush`, `DockSurfaceEditorBrush`, `DockSurfacePanelBrush`, `DockSurfaceHeaderBrush`, `DockSurfaceHeaderActiveBrush` |
| Borders/structure | `DockBorderSubtleBrush`, `DockBorderStrongBrush`, `DockSeparatorBrush`, `DockSplitterIdleBrush`, `DockSplitterHoverBrush`, `DockSplitterDragBrush`, `DockTargetIndicatorBrush` |
| Tabs | `DockTabBackgroundBrush`, `DockTabHoverBackgroundBrush`, `DockTabActiveBackgroundBrush`, `DockTabActiveIndicatorBrush`, `DockTabForegroundBrush`, `DockTabSelectedForegroundBrush`, `DockTabActiveForegroundBrush`, `DockTabCloseHoverBackgroundBrush` |
| Chrome buttons | `DockChromeButtonForegroundBrush`, `DockChromeButtonHoverBackgroundBrush`, `DockChromeButtonPressedBackgroundBrush`, `DockChromeButtonDangerHoverBrush` |
| Density/shape | `DockCornerRadiusSmall`, `DockHeaderHeight`, `DockTabHeight`, `DockTabHorizontalPadding`, `DockIconSizeSmall`, `DockIconSizeNormal` |
| Density controls | `DockTabItemMinHeight`, `DockToolTabItemMinHeight`, `DockDocumentTabItemPadding`, `DockToolTabItemPadding`, `DockToolTabItemSelectedPadding`, `DockCreateButtonWidth`, `DockCreateButtonHeight`, `DockCloseButtonSize`, `DockChromeButtonWidth`, `DockChromeButtonHeight`, `DockChromeButtonPadding`, `DockChromeButtonMargin`, `DockMdiTitleIconSize`, `DockMdiHeaderDragPadding` |
| Tab/header metrics | `DockHeaderContentPadding`, `DockModifiedIndicatorMargin`, `DockTabContentSpacing`, `DockDocumentTabHeaderContentSpacing`, `DockTabContentMargin`, `DockCreateButtonIconMargin` |
| Chrome/MDI metrics | `DockToolChromeHeaderMargin`, `DockToolChromeTitleMargin`, `DockToolChromeMenuIconMargin`, `DockToolChromeDividerThickness`, `DockChromeGripHeight`, `DockChromeGripMargin`, `DockChromeGripBrush`, `DockMdiHeaderColumnSpacing`, `DockMdiButtonStripSpacing`, `DockMdiButtonStripMargin`, `DockMdiResizeEdgeThickness`, `DockMdiResizeCornerSize` |
| Selector/target metrics | `DockTargetSelectorSize`, `DockTargetSelectorGridMaxSize`, `DockSelectorOverlayBackdropBrush`, `DockSelectorOverlayCornerRadius`, `DockSelectorOverlayPadding`, `DockSelectorOverlayMinWidth`, `DockSelectorOverlayMaxWidth`, `DockSelectorOverlaySpacing`, `DockSelectorOverlayListMinHeight`, `DockSelectorOverlayListMaxHeight`, `DockSelectorOverlayItemPadding`, `DockSelectorOverlayItemCornerRadius`, `DockSelectorOverlayBadgeSpacing`, `DockSelectorOverlayBadgeMargin`, `DockSelectorOverlayBadgeCornerRadius`, `DockSelectorOverlayBadgePadding`, `DockSelectorOverlayBadgeFontSize` |
| Command/preview metrics | `DockCommandBarPadding`, `DockCommandBarSpacing`, `DockDragPreviewCornerRadius`, `DockDragPreviewHeaderPadding`, `DockDragPreviewHeaderSpacing`, `DockDragPreviewStatusSpacing`, `DockDragPreviewStatusIconSize`, `DockHostTitleBarMouseTrackerHeight` |
| Overlay/dialog metrics | `DockOverlayReloadButtonMargin`, `DockOverlayCardCornerRadius`, `DockOverlayCardPadding`, `DockOverlayCardSpacing`, `DockOverlayMessageFontSize`, `DockOverlayProgressWidth`, `DockOverlayProgressHeight`, `DockDialogCornerRadius`, `DockDialogPadding`, `DockDialogMinWidth`, `DockDialogMaxWidth`, `DockDialogSpacing`, `DockDialogTitleFontSize`, `DockDialogCloseButtonSize`, `DockConfirmationDialogPadding`, `DockConfirmationDialogMaxWidth`, `DockConfirmationDialogStackSpacing`, `DockConfirmationDialogActionsSpacing` |

## Default mapping

Defaults are defined in:

- `src/Dock.Avalonia.Themes.Fluent/Accents/Fluent.axaml`
- `src/Dock.Avalonia.Themes.Simple/Accents/Simple.axaml`

These defaults map to existing Dock theme resources so current apps keep their prior look unless you override tokens.

## Override example

```xaml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <SolidColorBrush x:Key="DockSurfaceHeaderBrush" Color="#FF2D2D30" />
  <SolidColorBrush x:Key="DockTabActiveIndicatorBrush" Color="#FF3794FF" />
  <SolidColorBrush x:Key="DockChromeButtonDangerHoverBrush" Color="#FFC94F4F" />
  <x:Double x:Key="DockHeaderHeight">24</x:Double>
</ResourceDictionary>
```

Merge this dictionary after `DockFluentTheme` or `DockSimpleTheme`.

## Compact density mode

Both Dock theme classes expose a `DensityStyle` property:

```xaml
<dockFluent:DockFluentTheme DensityStyle="Compact" />
<dockSimple:DockSimpleTheme DensityStyle="Compact" />
```

Compact mode loads:

- `avares://Dock.Avalonia.Themes.Fluent/DensityStyles/Compact.axaml`
- `avares://Dock.Avalonia.Themes.Simple/DensityStyles/Compact.axaml`
