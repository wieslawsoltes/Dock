# Pinned dock window

`DockSettings` exposes an option to display auto-hidden tools in a transparent floating window. This works around native control airspace issues just like floating dock adorners.

```csharp
// Enable floating auto-hide windows
DockSettings.UsePinnedDockWindow = true;
```

When enabled the `PinnedDockControl` places the preview content inside a lightweight `PinnedDockWindow`. The window follows the host layout and closes automatically when the tool is hidden.
