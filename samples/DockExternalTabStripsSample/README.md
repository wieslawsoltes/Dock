# Dock External Tab Strips Sample

This sample demonstrates the contract-based external surface API for document tabs:

- `IExternalDockSurface`
- `DockControl.RegisterExternalDockSurface(...)`
- `DockControl.UnregisterExternalDockSurface(...)`

## What it showcases

- Two independent `DockControl` workspaces.
- One external `DocumentTabStrip` per workspace (two total external strips).
- Each workspace is rooted by a `DocumentDock` target.
- Internal `DocumentControl` tab strips hidden; external strips are the active tab UI.
- Drag and drop from external tab strips into docking targets, including cross-workspace docking.

## Run

```bash
dotnet run --project samples/DockExternalTabStripsSample/DockExternalTabStripsSample.csproj
```

## Build checks

```bash
dotnet build samples/DockExternalTabStripsSample/DockExternalTabStripsSample.csproj
dotnet build Dock.slnx
```
