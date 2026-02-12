# Dock Simple Theme

This guide documents the Dock Simple theme in detail, including architecture, density support, and token/preset customization.

## Overview

`DockSimpleTheme` is the Dock theme for applications using Avalonia `SimpleTheme`.

```xaml
<Application.Styles>
  <SimpleTheme />
  <dockSimple:DockSimpleTheme />
</Application.Styles>
```

Theme implementation:

- `src/Dock.Avalonia.Themes.Simple/DockSimpleTheme.axaml`
- `src/Dock.Avalonia.Themes.Simple/DockSimpleTheme.axaml.cs`
- `src/Dock.Avalonia.Themes.Simple/Accents/Simple.axaml`

## Architecture notes

Dock Simple uses its own accent resources but shares Dock control template XAML with Fluent.

This is implemented by linking Fluent control `.axaml` files into the Simple theme assembly:

- `src/Dock.Avalonia.Themes.Simple/Dock.Avalonia.Themes.Simple.csproj`

Practical result:

- same control template behavior,
- same semantic token contract,
- different default accent mappings.

## Density support

`DockSimpleTheme` supports:

- `DensityStyle="Normal"` (default),
- `DensityStyle="Compact"`.

```xaml
<dockSimple:DockSimpleTheme DensityStyle="Compact" />
```

Compact density resource dictionary:

- `avares://Dock.Avalonia.Themes.Simple/DensityStyles/Compact.axaml`

## Simple token customization

Override semantic tokens after `DockSimpleTheme`:

```xaml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary>
        <SolidColorBrush x:Key="DockSurfacePanelBrush" Color="#FFF0F0F0" />
        <SolidColorBrush x:Key="DockTabActiveIndicatorBrush" Color="#FF4A8BFF" />
        <SolidColorBrush x:Key="DockBorderSubtleBrush" Color="#FFD3D3D3" />
      </ResourceDictionary>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

For the complete token list, see [Dock Theme Design Tokens](dock-theme-design-tokens.md).

## IDE preset usage with Simple

Simple-specific preset dictionaries:

- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeLight.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderLight.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml`

```xaml
<Application.Styles>
  <SimpleTheme />
  <dockSimple:DockSimpleTheme DensityStyle="Compact" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderLight.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

## Recommended customization order

1. Apply `DockSimpleTheme`.
2. Choose density.
3. Optionally merge an IDE preset.
4. Add app-specific token overrides at highest priority.

## Related docs

- [Dock Theme Design Tokens](dock-theme-design-tokens.md)
- [Dock IDE Presets](dock-theme-ide-presets.md)
- [Dock Theme Token Migration](dock-theme-token-migration.md)
