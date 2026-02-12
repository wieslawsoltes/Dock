# BrowserTabTheme Sample

This sample backports a browser-tab visual style from StackWich into the original Dock library as a sample-local theme overlay.

## What this sample demonstrates

- Browser-like document and tool tab visuals.
- Custom window chrome styling via `HostWindowTitleBar` and custom `CaptionButtons`.
- Dock drag/drop, float, pin, and document creation behavior preserved.
- Theme dictionaries for both Light (`Default`) and Dark variants.

## What is intentionally excluded

- SW app-shell integrations (sidebar, banner, premium, feedback, custom host behaviors).
- Any modifications to `DockFluentTheme` in `src/`.

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
- Custom title bar and caption buttons.
- Dock target indicators and drag preview visuals.
