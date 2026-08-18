# BrowserTabTheme Sample

This sample backports a browser-tab visual style from StackWich into the original Dock library, using the reusable `Dock.Avalonia.Themes.Browser` theme package.

## What this sample demonstrates

- Browser-like document and tool tab visuals.
- Browser-style window chrome driven by Avalonia drawn decorations and Dock theme resources.
- Dock drag/drop, including dropping a tab onto a side target to create a new
  horizontal or vertical document dock.
- Native main-window chrome while document layout mode is MDI, keeping maximized
  MDI children clear of the operating-system caption controls.
- Float, pin, and document creation behavior preserved.
- Theme dictionaries for both Light (`Default`) and Dark variants.

## What is intentionally excluded

- SW app-shell integrations (sidebar, banner, premium, feedback, custom host behaviors).
- Any modifications to `DockFluentTheme` in `src/`.

## Theme package source

- `src/Dock.Avalonia.Themes.Browser/Dock.Avalonia.Themes.Browser.csproj`
- `src/Dock.Avalonia.Themes.Browser/Styles/`

## Run

```bash
dotnet run --project samples/BrowserTabTheme/BrowserTabTheme.csproj
```

## Build checks

```bash
dotnet build samples/BrowserTabTheme/BrowserTabTheme.csproj
dotnet build Dock.slnx
```

## Screenshot

Capture a screenshot after launch to validate:

- Browser-style tab states: default, hover, selected, selected+hover.
- Custom drawn caption buttons aligned with the browser-style chrome.
- Dock target indicators and drag preview visuals.
