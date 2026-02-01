# Pinned dock window

`DockSettings` exposes an option to display auto-hidden tools in a transparent floating window. This works around native control airspace issues just like floating dock adorners.

```csharp
// Enable floating auto-hide windows
DockSettings.UsePinnedDockWindow = true;
```

When enabled the `PinnedDockControl` places the preview content inside a lightweight `PinnedDockWindow`. The window follows the host layout and closes automatically when the tool is hidden.

In managed window mode (`DockSettings.UseManagedWindows = true`), pinned windows render inside `ManagedWindowLayer` instead of a native window.

## Alignment and orientation

`PinnedDockControl` exposes `PinnedDockAlignment` to control which edge the pinned preview occupies. The default templates bind it to the owning `ToolDock.Alignment`, but you can override it when building custom templates.

Pinned tab strips use `ToolPinnedControl.Orientation` (propagated to `ToolPinItemControl.Orientation`) to arrange pinned tabs vertically or horizontally.

## Pinned tab behavior

Pinned tab items toggle the preview for the selected tool. Clicking the same pinned tab again hides the preview instead of closing the dockable. Use the pinned tab context menu (or `IFactory.CloseDockable`) when you want to close or remove a pinned tool.
