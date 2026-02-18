# Dock Browser Theme

This guide documents the browser-tab style Dock theme hosted in the Dock source tree.

## Overview

`BrowserTabTheme` is a reusable theme class in:

- `src/Dock.Avalonia.Themes.Browser/Dock.Avalonia.Themes.Browser.csproj`

It is designed as an overlay theme on top of `DockFluentTheme` and applies browser-style tabs, title bar styling, and related Dock control templates.

## Apply the theme

Reference the browser theme project/package and use it in `App.axaml`:

```xaml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:browserTheme="using:Dock.Avalonia.Themes.Browser">
  <Application.Styles>
    <FluentTheme />
    <DockFluentTheme />
    <browserTheme:BrowserTabTheme />
  </Application.Styles>
</Application>
```

## Density support

`BrowserTabTheme` supports Dock density switching with `DensityStyle`:

```xaml
<browserTheme:BrowserTabTheme DensityStyle="Compact" />
```

Compact density dictionary:

- `avares://Dock.Avalonia.Themes.Browser/Styles/DensityStyles/Compact.axaml`

## Theme resources

Primary dictionaries:

- `avares://Dock.Avalonia.Themes.Browser/Styles/BrowserTabAccents.axaml`
- `avares://Dock.Avalonia.Themes.Browser/Styles/Controls/AllResources.axaml`

## Related docs

- [Fluent theme guide](dock-theme-fluent.md)
- [Custom themes](dock-custom-theme.md)
- [BrowserTabTheme sample](dock-theme-browser-sample.md)
