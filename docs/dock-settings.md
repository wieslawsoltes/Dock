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

## Global docking

`DockSettings.EnableGlobalDocking` controls whether dockables can be dropped
onto other `DockControl` instances. If set to `false` the global dock target is
hidden and drags are limited to the originating control.

## App builder integration

You can configure the settings when building your Avalonia application:

```csharp
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .WithDockSettings(useFloatingDockAdorner: true,
                      minimumHorizontalDragDistance: 6);
```

## Hide on close

`FactoryBase` exposes two properties that control whether closing a tool or
document hides it instead of removing it. Hidden items can be restored through
the factory helpers:

```csharp
var factory = new MyFactory
{
    HideToolsOnClose = true,
    HideDocumentsOnClose = true
};
```

Both options are disabled by default. When enabled the `CloseDockable` command
moves the dockable to the `IRootDock.HiddenDockables` collection instead of
deleting it.

## Prevent closing the last dockable

Each dock exposes a `CanCloseLastDockable` property. When set to `false`
the `CloseDockable` command ignores requests that would remove the final
visible item from that dock.

For more details on dockable properties see [Dockable Property Settings](dock-dockable-properties.md).
