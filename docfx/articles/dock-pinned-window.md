# Pinned dock window

`DockSettings` exposes an option to display auto-hidden tools in a transparent floating window. This works around native control airspace issues just like floating dock adorners.

```csharp
// Enable floating auto-hide windows
DockSettings.UsePinnedDockWindow = true;
```

When enabled the `PinnedDockControl` places the preview content inside a lightweight `PinnedDockWindow`. The window follows the host layout and closes automatically when the tool is hidden.

## Alignment and orientation

`PinnedDockControl` exposes `PinnedDockAlignment` to control which edge the pinned preview occupies. The default templates bind it to the owning `ToolDock.Alignment`, but you can override it when building custom templates.

Pinned tab strips use `ToolPinnedControl.Orientation` (propagated to `ToolPinItemControl.Orientation`) to arrange pinned tabs vertically or horizontally.
