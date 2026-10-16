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
    <browserTheme:BrowserTabTheme CacheDocumentTabContent="True" />
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

## Document tab content caching

`BrowserTabTheme` exposes `CacheDocumentTabContent` to keep document views alive while switching tabs:

```xaml
<browserTheme:BrowserTabTheme CacheDocumentTabContent="True" />
```

## Theme resources

Primary dictionaries:

- `avares://Dock.Avalonia.Themes.Browser/Styles/BrowserTabAccents.axaml`
- `avares://Dock.Avalonia.Themes.Browser/Styles/Controls/AllResources.axaml`

## Docking previews

Browser uses its own local `DockTarget` template: transparent edge regions select
left/right/top/bottom splits and the central region merges tabs. Only the proposed
area is painted, using `DockTargetIndicatorBrush`; the Fluent PNG selector cross
is not constructed. Document splitting remains enabled by default.

For a tab strip that only merges tabs, set `DockProperties.ShowDockIndicatorOnly="True"`
and `DockProperties.IndicatorDockOperation="Fill"`. This previews the whole target
area without changing the application's allowed docking operations.

This changes the Browser theme's local split affordance from small central icons
to edge regions. Applications that intentionally prefer the Fluent selector can
supply that `ControlTheme` through `DockTarget.TargetTheme`. Global docking targets
continue to use the existing global theme.

## Related docs

- [Fluent theme guide](dock-theme-fluent.md)
- [Custom themes](dock-custom-theme.md)
- [BrowserTabTheme sample](dock-theme-browser-sample.md)
