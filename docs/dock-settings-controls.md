# DockSettings in Controls

`Dock.Settings` defines global properties that help you implement consistent drag and drop behaviour. This guide explains why these settings matter and how to apply them when creating custom controls or overriding templates.

## Why use DockSettings?

All Dock controls start a drag operation only after the pointer moves a short distance. The thresholds are exposed as `DockSettings.MinimumHorizontalDragDistance` and `DockSettings.MinimumVerticalDragDistance`. By relying on these values you ensure every control reacts the same way and users do not accidentally begin dragging.

When dragging tabs you may want to float them only after moving further away from the strip. `DockSettings.MinimumTabFloatDistance` defines how far the pointer must travel before the tab detaches.

When you build your own controls or replace the default templates you should respect these distances instead of hard coding your own numbers.

## Checking drag thresholds

The `DocumentTabStrip` control uses the settings when it decides whether a pointer movement counts as a drag:

```csharp
var delta = currentPoint - _dragStartPoint;
if (!(Math.Abs(delta.X) > DockSettings.MinimumHorizontalDragDistance)
    && !(Math.Abs(delta.Y) > DockSettings.MinimumVerticalDragDistance))
{
    return; // Not enough movement yet
}
```

You can apply the same check in your custom `PointerMoved` handlers before calling `BeginMoveDrag` on the host window.

## Marking drag and drop areas

-`DockProperties` exposes attached properties to opt‑in to docking features:

- `IsDockTarget` designates the surface that accepts docked items and shows a `DockTargetBase` adorner.
- `IsDragArea` marks a region where the user can start dragging.
- `IsDropArea` marks a region that can receive drops.
- `IsDragEnabled` and `IsDropEnabled` allow you to enable or disable interactions globally for any subtree.

These properties are normally set in the default styles but you can place them on your own controls:

```xml
<Border xmlns:dockSettings="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dockSettings:DockProperties.IsDragArea="True"
        dockSettings:DockProperties.IsDropArea="True" />
```

From code you can toggle interactions dynamically:

```csharp
DockProperties.SetIsDragEnabled(myControl, false);
DockProperties.SetIsDropEnabled(myControl, false);
```

## Guidelines

1. Use the `DockSettings` drag distances whenever you implement pointer logic for dragging dockables.
2. Apply `DockProperties` to identify drag handles and drop zones in your templates.
3. Consider disabling drag or drop interactions with `IsDragEnabled` and `IsDropEnabled` when your UI needs to lock down docking temporarily.

For an overview of all settings see [Dock settings](dock-settings.md) and the other guides listed in the documentation index.
