# Dock Settings

`Dock.Settings` exposes global properties and constants that control drag and drop behaviour.
Use these when you need to adjust interaction distances or override the default templates.

## Attached properties

`DockProperties` defines several attached properties that can be applied to any Avalonia control.
They are used by the default control templates but you can set them manually:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `IsDockTarget` | `bool` | Marks a control as a docking surface. |
| `IsDragArea` | `bool` | Identifies a region that allows starting a drag. |
| `IsDropArea` | `bool` | Identifies a region that can accept dropped dockables. |
| `IsDragEnabled` | `bool` | Globally enables or disables dragging for child dockables. |
| `IsDropEnabled` | `bool` | Globally enables or disables dropping of dockables. |

Example disabling drag and drop for an entire window:

```xml
<Window xmlns:dockSettings="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dockSettings:DockProperties.IsDragEnabled="False"
        dockSettings:DockProperties.IsDropEnabled="False">
    <DockControl />
</Window>
```

## Drag thresholds

`DockSettings` contains two public fields controlling how far the pointer must move
before a drag operation begins:

```csharp
DockSettings.MinimumHorizontalDragDistance = 4;
DockSettings.MinimumVerticalDragDistance = 4;
```

Increase these values if small pointer movements should not initiate dragging.

For more details on dockable properties see [Dockable Property Settings](dock-dockable-properties.md).
