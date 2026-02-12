# BrowserTabTheme Sample

The BrowserTabTheme sample demonstrates how to consume `Dock.Avalonia.Themes.Browser` from an application project.

## Sample project

- `samples/BrowserTabTheme/BrowserTabTheme.csproj`
- `samples/BrowserTabTheme/App.axaml`

The sample references the reusable theme project from `src` and applies it with:

```xaml
<FluentTheme />
<DockFluentTheme />
<themes:BrowserTabTheme />
```

## Run the sample

```bash
dotnet run --project samples/BrowserTabTheme/BrowserTabTheme.csproj
```

## Build checks

```bash
dotnet build src/Dock.Avalonia.Themes.Browser/Dock.Avalonia.Themes.Browser.csproj
dotnet build samples/BrowserTabTheme/BrowserTabTheme.csproj
```

## What to validate

- Browser-style document and tool tabs.
- Host window title bar and caption buttons.
- Dock targets and drag-preview visuals.
- Floating windows and pin/unpin interactions.
