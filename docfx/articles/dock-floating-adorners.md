# Floating Dock Adorners

`DockSettings` exposes an option to display dock target adorners in a transparent floating window. This avoids so-called *airspace* issues when dragging over embedded native controls.

```csharp
// Enable floating adorners
DockSettings.UseFloatingDockAdorner = true;
```

```csharp
// Via the app builder
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseFloatingDockAdorner();
```

When enabled `AdornerHelper` creates a lightweight `DockAdornerWindow` positioned above the drag source. The window is transparent and not topmost, so the drag preview window still appears above the adorner because only the preview is marked as `Topmost`.

Use this option if dock targets fail to appear when dragging over native controls or popups. The default value is `false` which uses an `AdornerLayer` inside the same window.
