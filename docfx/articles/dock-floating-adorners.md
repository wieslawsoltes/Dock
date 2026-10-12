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

In managed window mode (`DockSettings.UseManagedWindows = true`), the floating adorner is rendered inside `ManagedWindowLayer` instead of a native window.

## Selectors in a custom overlay template

An overlay can keep its selectors outside the native `DockTarget` template by
assigning `DockTargetBase.Presentation` to an `IDockTargetPresentation`.
`GetSelector` supplies the control for each operation; returning null omits that
operation. Dock translates pointer coordinates, applies direction and edge
limits, and calls the existing visibility and drop-validation policies.
`SetOperationState` receives the resulting selector visibility and active preview
state. The controls can belong to another window and can have render transforms.

The presentation owns the custom visuals. Use opacity for policy-driven selector
visibility so the controls retain their layout; an explicitly invisible selector
cannot be activated. Indicator-only mode hides selectors while keeping the
chosen preview active. Reset clears all reported states, and replacing the
presentation clears the previous presentation before another hover is evaluated.
Clear `Presentation` when its template or window is discarded. A null value
restores the ordinary native template and its registered selectors.
