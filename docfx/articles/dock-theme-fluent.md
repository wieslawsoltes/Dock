# Dock Fluent Theme

This guide documents the Dock Fluent theme in detail: structure, density modes, token customization, and IDE preset integration.

## Overview

`DockFluentTheme` is the primary Dock theme for Fluent-based applications.

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme />
</Application.Styles>
```

Theme implementation:

- `src/Dock.Avalonia.Themes.Fluent/DockFluentTheme.axaml`
- `src/Dock.Avalonia.Themes.Fluent/DockFluentTheme.axaml.cs`
- `src/Dock.Avalonia.Themes.Fluent/Accents/Fluent.axaml`

## What DockFluentTheme includes

`DockFluentTheme` merges:

- accent and string resources,
- window chrome resources,
- all Dock control templates (documents, tools, MDI, overlays, host windows).

Key control template resources include:

- `DocumentControl.axaml`
- `DocumentTabStrip.axaml`
- `DocumentTabStripItem.axaml`
- `ToolChromeControl.axaml`
- `ToolTabStrip.axaml`
- `ToolTabStripItem.axaml`
- `MdiDocumentWindow.axaml`

## Density support

`DockFluentTheme` supports two density modes:

- `DensityStyle="Normal"` (default),
- `DensityStyle="Compact"`.

```xaml
<dockFluent:DockFluentTheme DensityStyle="Compact" />
```

Compact density resource dictionary:

- `avares://Dock.Avalonia.Themes.Fluent/DensityStyles/Compact.axaml`

Compact mode reduces tab/button/icon metrics by overriding density tokens (for example `DockTabItemMinHeight`, `DockCloseButtonSize`, `DockChromeButtonWidth`).

## Document tab content caching

`DockFluentTheme` can keep document tab content alive instead of recreating it on each tab switch:

```xaml
<dockFluent:DockFluentTheme CacheDocumentTabContent="True" />
```

When enabled, document views remain instantiated while hidden tabs are inactive, which can improve tab-switch latency for heavy views.

## Fluent token customization

Customize Dock Fluent visuals by overriding semantic tokens after `DockFluentTheme`.

```xaml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary>
        <SolidColorBrush x:Key="DockSurfaceHeaderBrush" Color="#FF2D2D30" />
        <SolidColorBrush x:Key="DockTabActiveIndicatorBrush" Color="#FF3794FF" />
        <SolidColorBrush x:Key="DockChromeButtonDangerHoverBrush" Color="#FFC94F4F" />
      </ResourceDictionary>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

For the complete token list, see [Dock Theme Design Tokens](dock-theme-design-tokens.md).

## IDE preset usage with Fluent

Fluent-specific preset dictionaries:

- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml`

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme DensityStyle="Compact" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

Presets override color/surface tokens only; density remains controlled by `DensityStyle`.

## Recommended customization order

1. Apply `DockFluentTheme`.
2. Pick density (`Normal` or `Compact`).
3. Optionally merge an IDE preset.
4. Add app-specific token overrides last.

## Related docs

- [Dock Theme Design Tokens](dock-theme-design-tokens.md)
- [Dock IDE Presets](dock-theme-ide-presets.md)
- [Dock Theme Token Migration](dock-theme-token-migration.md)
