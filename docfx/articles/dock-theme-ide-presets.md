# Dock IDE Presets

Dock ships token-only IDE presets for Fluent and Simple themes.  
They override semantic tokens only and do not replace Dock control templates.

## Preset files

Fluent:
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml`
- `avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml`

Simple:
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeLight.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderLight.axaml`
- `avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml`

## Usage

### Fluent + Default

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme DensityStyle="Normal" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Fluent + VS Code Dark

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

### Fluent + Rider Light

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme DensityStyle="Normal" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Fluent + VS Code Light

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme DensityStyle="Normal" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Fluent + Rider Dark

```xaml
<Application.Styles>
  <FluentTheme />
  <dockFluent:DockFluentTheme DensityStyle="Compact" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Simple + Default

```xaml
<Application.Styles>
  <SimpleTheme />
  <dockSimple:DockSimpleTheme DensityStyle="Normal" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Simple + VS Code Dark

```xaml
<Application.Styles>
  <SimpleTheme />
  <dockSimple:DockSimpleTheme DensityStyle="Compact" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

### Simple + Rider Dark

```xaml
<Application.Styles>
  <SimpleTheme />
  <dockSimple:DockSimpleTheme DensityStyle="Compact" />
</Application.Styles>
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceInclude Source="avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

## Customization workflow

1. Start from one preset.
2. Copy it into your app.
3. Adjust only token values you need.
4. Keep your dictionary after the Dock theme include so overrides win.

For the full token list, see [Dock Theme Semantic Tokens](dock-theme-semantic-tokens.md).

IDE presets intentionally override color/surface tokens only.  
Density continues to come from `DockFluentTheme.DensityStyle` and `DockSimpleTheme.DensityStyle`.

Use `Default.axaml` when you want preset-driven runtime switching but need to return to the base Dock theme token mapping.
