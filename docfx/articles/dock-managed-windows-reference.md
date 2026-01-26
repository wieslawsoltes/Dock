# Managed Windows Reference

This reference summarizes the main settings and types involved in managed window hosting.

## Settings

| Setting | Description |
| --- | --- |
| `DockSettings.UseManagedWindows` | Use in-app managed windows for floating dock windows. |
| `DockSettings.EnableWindowMagnetism` | Snap managed windows to nearby windows when dragging. |
| `DockSettings.WindowMagnetDistance` | Snap distance in pixels for managed window magnetism. |
| `DockSettings.BringWindowsToFrontOnDrag` | Activate managed windows when dragging begins. |
| `DockSettings.ShowDockablePreviewOnDrag` | Render dockable content inside drag previews. |
| `DockSettings.DragPreviewOpacity` | Opacity for drag previews in managed mode. |
| `DockSettings.UseOwnerForFloatingWindows` | Applies to native windows only. |

## App builder and options

| API | Description |
| --- | --- |
| `AppBuilderExtensions.UseManagedWindows` | Enables managed windows in the app builder. |
| `DockSettingsOptions.UseManagedWindows` | Nullable option used by `WithDockSettings`. |

## DockControl integration

| API | Description |
| --- | --- |
| `DockControl.EnableManagedWindowLayer` | Enables registration and display of the managed layer. |
| `DockControl.HostWindowFactory` | Override host window creation. Return `ManagedHostWindow` in managed mode. |

## Managed window types

| Type | Description |
| --- | --- |
| `ManagedHostWindow` | `IHostWindow` implementation that inserts a managed window into the layer. |
| `ManagedWindowLayer` | `TemplatedControl` that hosts managed windows and overlays. |
| `ManagedWindowDock` | Dock used to track managed floating windows. |
| `ManagedDockWindowDocument` | `IMdiDocument` wrapper around `IDockWindow` with `MdiBounds`, `MdiState`, and `MdiZIndex`. |

## MDI integration

| Type | Description |
| --- | --- |
| `MdiDocumentWindow` | Control that renders managed floating windows. |
| `IMdiLayoutManager` | Strategy for arranging MDI windows. |
| `ClassicMdiLayoutManager` | Default managed window layout manager. |
