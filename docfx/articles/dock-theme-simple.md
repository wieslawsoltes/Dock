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

Keep the base Avalonia theme first and the matching Dock theme second. Do not
load `FluentTheme` and `SimpleTheme` together.

## Switch DockMvvmSample from Fluent to Simple

`DockMvvmSample` uses Fluent by default. The theme boundary is confined to the
composition root, so its views and view models do not need theme-specific
changes.

### 1. Change the theme package import

In `samples/DockMvvmSample/DockMvvmSample.csproj`, replace:

```xml
<Import Project="..\..\build\Avalonia.Themes.Fluent.props" />
```

with:

```xml
<Import Project="..\..\build\Avalonia.Themes.Simple.props" />
```

The Simple props file includes both `Avalonia.Themes.Simple` and
`Avalonia.Fonts.Inter`; the sample's views continue to use the Inter font asset.

Replace the Dock theme project reference as well:

```xml
<ProjectReference Include="..\..\src\Dock.Avalonia.Themes.Simple\Dock.Avalonia.Themes.Simple.csproj" />
```

Remove the corresponding `Dock.Avalonia.Themes.Fluent` project reference.

Applications using Central Package Management need this entry (the repository
already defines it in `Directory.Packages.props`):

```xml
<PackageVersion Include="Avalonia.Themes.Simple" Version="$(AvaloniaVersion)" />
```

### 2. Change App.axaml

Use the Simple preset URI, then replace the two theme styles:

```xaml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml" />
      <!-- Keep the sample's other dictionaries after the preset. -->
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>

<Application.Styles>
  <SimpleTheme />
  <DockSimpleTheme DensityStyle="Normal" />

  <!-- Keep the sample's app-specific styles after both themes. -->
</Application.Styles>
```

The order is intentional: base theme, Dock theme, then app-specific overrides.
Preset dictionaries belong in application resources and their assembly URI must
match the selected Dock theme.

### 3. Change the theme manager

In `App.axaml.cs`, replace the Fluent namespace and manager:

```csharp
using Dock.Avalonia.Themes.Simple;

// In App.Initialize():
ThemeManager = new DockSimpleThemeManager();
```

The existing theme and preset controls in `MainView` continue to work because
both managers implement `IDockThemeManager`.

### 4. Build the converted sample

```bash
dotnet build samples/DockMvvmSample/DockMvvmSample.csproj -c Release
```

The document-tab overflow buttons are supported by the current Simple theme;
the missing scrollbar resources reported in #1025 were added in Dock 12.

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

## Document tab content caching

`DockSimpleTheme` can keep document tab content alive instead of recreating it on each tab switch:

```xaml
<dockSimple:DockSimpleTheme CacheDocumentTabContent="True" />
```

This uses the shared document control templates and keeps hidden tab views instantiated.

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
