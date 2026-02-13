# Dock Theme Design Tokens

Dock themes expose semantic design tokens that decouple control templates from raw color/size keys.

All token keys below are available in both:

- `DockFluentTheme`
- `DockSimpleTheme`

## Surface tokens

| Token | Purpose |
|---|---|
| `DockSurfaceWorkbenchBrush` | Main background surface behind docks. |
| `DockSurfaceSidebarBrush` | Sidebar/tool-area surface. |
| `DockSurfaceEditorBrush` | Document/editor surface. |
| `DockSurfacePanelBrush` | Secondary panel surface. |
| `DockSurfaceHeaderBrush` | Header/chrome background. |
| `DockSurfaceHeaderActiveBrush` | Active header/chrome background. |

## Border and structure tokens

| Token | Purpose |
|---|---|
| `DockBorderSubtleBrush` | Standard low-emphasis border color. |
| `DockBorderStrongBrush` | Stronger border color (optional use). |
| `DockSeparatorBrush` | Thin separators and strip dividers. |
| `DockSplitterIdleBrush` | Splitter idle color. |
| `DockSplitterHoverBrush` | Splitter hover color. |
| `DockSplitterDragBrush` | Splitter drag color. |
| `DockTargetIndicatorBrush` | Dock target overlay indicator color. |

## Tab tokens

| Token | Purpose |
|---|---|
| `DockTabBackgroundBrush` | Base tab strip background. |
| `DockTabHoverBackgroundBrush` | Tab hover background. |
| `DockTabActiveBackgroundBrush` | Active/selected tab background. |
| `DockTabActiveIndicatorBrush` | Active tab indicator line/accent. |
| `DockTabForegroundBrush` | Default tab text/icon color. |
| `DockTabSelectedForegroundBrush` | Selected tab text color (non-active accent). |
| `DockTabActiveForegroundBrush` | Active tab text/icon color. |
| `DockTabCloseHoverBackgroundBrush` | Document close button hover background. |
| `DockDocumentTabStripBackgroundBrush` | Document tab strip background brush. |
| `DockDocumentTabSelectedForegroundBrush` | Selected document-tab foreground override used by `DocumentTabStripItem`. |
| `DockDocumentTabPointerOverForegroundBrush` | Pointer-over document-tab foreground override. |
| `DockDocumentTabCloseSelectedForegroundBrush` | Selected document-tab close icon foreground override. |
| `DockDocumentTabClosePointerOverForegroundBrush` | Pointer-over document-tab close icon foreground override. |

## Chrome button tokens

| Token | Purpose |
|---|---|
| `DockChromeButtonForegroundBrush` | Tool/MDI chrome icon color. |
| `DockChromeButtonHoverBackgroundBrush` | Chrome button hover background. |
| `DockChromeButtonPressedBackgroundBrush` | Chrome button pressed background. |
| `DockChromeButtonDangerHoverBrush` | Close/danger button hover background. |

## Global shape/density tokens

| Token | Purpose |
|---|---|
| `DockCornerRadiusSmall` | Shared small corner radius. |
| `DockHeaderHeight` | Header/chrome nominal height token. |
| `DockTabHeight` | Tab height token. |
| `DockTabHorizontalPadding` | Horizontal tab spacing token. |
| `DockIconSizeSmall` | Small icon size token. |
| `DockIconSizeNormal` | Normal icon size token. |

## Control density tokens

| Token | Purpose |
|---|---|
| `DockTabItemMinHeight` | Min height for document/tool tab items. |
| `DockDocumentTabItemMinHeight` | Min height specifically for document tab items. |
| `DockToolTabItemMinHeight` | Min height for tool tab items. |
| `DockDocumentTabStripMinHeight` | Min height for the document tab strip container. |
| `DockDocumentTabStripSeparatorSize` | Thickness of the document tab-strip separator line (`PART_DocumentSeperator`). |
| `DockDocumentTabStripLeadingSpacerWidth` | Width of optional leading spacer in document tab strip. |
| `DockDocumentTabItemMargin` | Outer margin for document tab items. |
| `DockDocumentTabItemPadding` | Document tab item padding. |
| `DockDocumentTabItemCornerRadius` | Corner radius for document tab items. |
| `DockDocumentTabCreateButtonCornerRadius` | Corner radius for the document create button. |
| `DockToolTabItemPadding` | Tool tab item padding. |
| `DockToolTabItemSelectedPadding` | Tool tab selected-state padding. |
| `DockCreateButtonWidth` | Document create button width. |
| `DockCreateButtonHeight` | Document create button height. |
| `DockCloseButtonSize` | Document close button size. |
| `DockChromeButtonWidth` | Tool/MDI chrome button width. |
| `DockChromeButtonHeight` | Tool/MDI chrome button height. |
| `DockChromeButtonPadding` | Tool/MDI chrome button padding. |
| `DockChromeButtonMargin` | Tool/MDI chrome button margin. |
| `DockMdiTitleIconSize` | MDI title bar icon viewbox size. |
| `DockMdiHeaderDragPadding` | MDI drag header padding. |

## Tab and header content metrics

| Token | Purpose |
|---|---|
| `DockHeaderContentPadding` | Shared title/header text padding in document/tool/MDI headers. |
| `DockModifiedIndicatorMargin` | Shared margin for modified marker (`*`). |
| `DockTabContentSpacing` | Spacing between tab content blocks (icon, title, modified, close). |
| `DockTabContentMargin` | Inner margin around tab content stack. |
| `DockCreateButtonIconMargin` | Plus/create icon margin in document tab strip. |
| `DockDocumentTabStripHorizontalCreateButtonMargin` | Horizontal create button margin in document tab strip. |
| `DockDocumentTabStripHorizontalScrollViewerMargin` | Horizontal scroll viewer margin in document tab strip. |
| `DockDocumentTabStripHorizontalScrollViewerPadding` | Horizontal scroll viewer padding in document tab strip. |

## Tool chrome and MDI metrics

| Token | Purpose |
|---|---|
| `DockToolChromeHeaderMargin` | Header dock panel outer margin in tool chrome. |
| `DockToolChromeTitleMargin` | Tool title text margin. |
| `DockToolChromeMenuIconMargin` | Menu icon margin in tool chrome button. |
| `DockToolChromeDividerThickness` | Divider line thickness below tool chrome header. |
| `DockChromeGripHeight` | Drag grip strip height in tool/MDI chrome. |
| `DockChromeGripMargin` | Drag grip strip margin in tool/MDI chrome. |
| `DockChromeGripBrush` | Shared tiled grip brush used by tool/MDI chrome. |
| `DockMdiHeaderColumnSpacing` | Column spacing in MDI document header layout. |
| `DockMdiButtonStripSpacing` | Spacing between MDI title bar buttons. |
| `DockMdiButtonStripMargin` | MDI button strip margin. |
| `DockMdiResizeEdgeThickness` | Hit target thickness for MDI edge resize handles. |
| `DockMdiResizeCornerSize` | Hit target size for MDI corner resize handles. |

## Selector and target metrics

| Token | Purpose |
|---|---|
| `DockTargetSelectorSize` | Dock target selector image size. |
| `DockTargetSelectorGridMaxSize` | Max size of local dock target selector grid. |
| `DockSelectorOverlayBackdropBrush` | Backdrop brush for selector overlay modal layer. |
| `DockSelectorOverlayCornerRadius` | Corner radius for selector overlay container. |
| `DockSelectorOverlayPadding` | Selector overlay container padding. |
| `DockSelectorOverlayMinWidth` | Selector overlay minimum width. |
| `DockSelectorOverlayMaxWidth` | Selector overlay maximum width. |
| `DockSelectorOverlaySpacing` | Vertical spacing in selector overlay content. |
| `DockSelectorOverlayListMinHeight` | Minimum list height in selector overlay. |
| `DockSelectorOverlayListMaxHeight` | Maximum list height in selector overlay. |
| `DockSelectorOverlayItemPadding` | List item padding in selector overlay. |
| `DockSelectorOverlayItemCornerRadius` | List item corner radius in selector overlay. |
| `DockSelectorOverlayBadgeSpacing` | Spacing between state badges. |
| `DockSelectorOverlayBadgeMargin` | Badge strip margin. |
| `DockSelectorOverlayBadgeCornerRadius` | Badge corner radius. |
| `DockSelectorOverlayBadgePadding` | Badge padding. |
| `DockSelectorOverlayBadgeFontSize` | Badge text size. |

## Command bar and drag preview metrics

| Token | Purpose |
|---|---|
| `DockCommandBarPadding` | Command bar container padding. |
| `DockCommandBarSpacing` | Vertical spacing between menu/tool/ribbon bands. |
| `DockDragPreviewCornerRadius` | Drag preview card corner radius. |
| `DockDragPreviewHeaderPadding` | Drag preview header padding. |
| `DockDragPreviewHeaderSpacing` | Header spacing between title and status block. |
| `DockDragPreviewStatusSpacing` | Spacing between drag status icon and text. |
| `DockDragPreviewStatusIconSize` | Drag status icon size. |
| `DockHostTitleBarMouseTrackerHeight` | Host title bar top tracker panel height. |
| `DockDocumentTabStripHorizontalCreateButtonDock` | Horizontal document-tab create button dock side (`Left`/`Right`). |
| `DockDocumentTabStripHorizontalCreateButtonAlignment` | Horizontal alignment for document-tab create button (`Left`/`Right`). |
| `DockDocumentControlIndicatorDockOperation` | Dock indicator operation for document control drop target (`Fill` etc.). |
| `DockDocumentTabStripHideWhenEmpty` | Whether empty non-create document tab strips are hidden. |
| `DockDocumentControlShowDockIndicatorOnly` | Whether document control dock target shows indicator-only rendering. |
| `DockDocumentTabStripSeparatorVisible` | Whether the document tab-strip separator (`PART_DocumentSeperator`) is visible. |
| `DockDocumentControlVerticalSpacing` | Vertical spacing for the `PART_DockPanel` in `DocumentControl`. |
| `DockDocumentContentBorderBrush` | Brush used by the document content border (`PART_Border`). |
| `DockDocumentContentBorderThickness` | Border thickness used by the document content border (`PART_Border`). |
| `DockDocumentTabStripHorizontalItemsPanel` | Horizontal items panel template for document tabs. |

## Overlay and dialog metrics

| Token | Purpose |
|---|---|
| `DockOverlayReloadButtonMargin` | Margin for the reload command button in busy overlay. |
| `DockOverlayCardCornerRadius` | Busy overlay card corner radius. |
| `DockOverlayCardPadding` | Busy overlay card padding. |
| `DockOverlayCardSpacing` | Busy overlay card content spacing. |
| `DockOverlayMessageFontSize` | Shared overlay message/title font size. |
| `DockOverlayProgressWidth` | Busy overlay progress width. |
| `DockOverlayProgressHeight` | Busy overlay progress height. |
| `DockDialogCornerRadius` | Dialog shell corner radius. |
| `DockDialogPadding` | Dialog shell padding. |
| `DockDialogMinWidth` | Dialog shell minimum width. |
| `DockDialogMaxWidth` | Dialog shell maximum width. |
| `DockDialogSpacing` | Dialog shell grid row/column spacing. |
| `DockDialogTitleFontSize` | Dialog and confirmation title font size. |
| `DockDialogCloseButtonSize` | Dialog shell close button size. |
| `DockConfirmationDialogPadding` | Confirmation dialog padding. |
| `DockConfirmationDialogMaxWidth` | Confirmation dialog maximum width. |
| `DockConfirmationDialogStackSpacing` | Confirmation dialog content spacing. |
| `DockConfirmationDialogActionsSpacing` | Confirmation dialog actions row spacing. |

## Where defaults are defined

- Fluent defaults: `src/Dock.Avalonia.Themes.Fluent/Accents/Fluent.axaml`
- Simple defaults: `src/Dock.Avalonia.Themes.Simple/Accents/Simple.axaml`
- Compact density overrides:
  - `src/Dock.Avalonia.Themes.Fluent/DensityStyles/Compact.axaml`
  - `src/Dock.Avalonia.Themes.Simple/DensityStyles/Compact.axaml`

## Token override pattern

```xaml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary>
        <SolidColorBrush x:Key="DockSurfaceHeaderBrush" Color="#FF2D2D30" />
        <SolidColorBrush x:Key="DockTabActiveIndicatorBrush" Color="#FF3794FF" />
        <x:Double x:Key="DockTabItemMinHeight">22</x:Double>
      </ResourceDictionary>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

## Related docs

- [Dock Fluent Theme](dock-theme-fluent.md)
- [Dock Simple Theme](dock-theme-simple.md)
- [Dock Theme Token Migration](dock-theme-token-migration.md)
