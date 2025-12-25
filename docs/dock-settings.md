# Dock Settings

`Dock.Settings` exposes global properties and constants that control drag and drop behaviour.
Use these when you need to adjust interaction distances or override the default templates.

## Attached properties

`DockProperties` defines several attached properties that can be applied to any Avalonia control.
They are used by the default control templates but you can set them manually:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `IsDockTarget` | `bool` | Marks a control as a docking surface that can display a `DockTargetBase` adorner. |
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

`IDock.EnableGlobalDocking` controls whether dockables can be dropped
onto a specific dock from outside its current dock control. This is useful for creating
multi-window applications where you can drag dockables between windows. Each dock
can individually control whether it accepts global docking operations, with the
default being enabled (true).

### Inheritance

The `EnableGlobalDocking` property is inherited down the dock hierarchy. When evaluating 
whether global docking should be allowed for a target dock, the system walks up the 
ownership chain checking all ancestor docks. If any ancestor dock has 
`EnableGlobalDocking = false`, global docking is disabled for the entire subtree.

This allows you to disable global docking for a large section of your docking layout 
by setting it to `false` on a parent dock. For example:

```csharp
// Disable global docking for an entire window/section
var parentDock = new ProportionalDock 
{ 
    EnableGlobalDocking = false 
};

// All child docks inherit the disabled setting
var childDock = new ToolDock 
{ 
    Owner = parentDock,
    EnableGlobalDocking = true  // This is overridden by parent's false
};
```

Note: the previous global setting and AppBuilder extension for enabling/disabling
global docking have been removed. Use the per-dock `IDock.EnableGlobalDocking`
property instead.

## Floating window owner

`DockSettings.UseOwnerForFloatingWindows` keeps floating windows above the main window by setting it as their owner.

## Floating dock adorners

`DockSettings.UseFloatingDockAdorner` enables showing drop indicators in a transparent floating window instead of overlays on the target controls. See [Floating Dock Adorners](dock-floating-adorners.md) for details.

## Pinned dock windows

`DockSettings.UsePinnedDockWindow` shows auto-hidden dockables inside a floating window instead of sliding panels. See [Pinned Dock Window](dock-pinned-window.md) for details.

## Window magnetism

`DockSettings.EnableWindowMagnetism` toggles snapping of floating windows. The snap distance
is controlled by `DockSettings.WindowMagnetDistance`.

## Bring windows to front on drag

`DockSettings.BringWindowsToFrontOnDrag` determines whether all floating windows
and any main window hosting a `DockControl` are activated when dragging begins.
Enabled by default.

## Selector hotkeys

Dock exposes a document and panel selector that can be toggled with keyboard gestures:

```csharp
DockSettings.SelectorEnabled = true;
DockSettings.DocumentSelectorKeyGesture = new KeyGesture(Key.Tab, KeyModifiers.Control);
DockSettings.ToolSelectorKeyGesture = new KeyGesture(Key.Tab, KeyModifiers.Control | KeyModifiers.Alt);
```

Disable `SelectorEnabled` to turn off the overlay entirely.

Dockables can also opt out or provide a custom label for the selector:

```csharp
document.ShowInSelector = false;
document.SelectorTitle = "Runtime Config";
```

## Command bar merging

Command bar merging is disabled by default. Enable it and choose a scope:

```csharp
DockSettings.CommandBarMergingEnabled = true;
DockSettings.CommandBarMergingScope = DockCommandBarMergingScope.ActiveDocument;
```

## App builder integration

You can configure the settings when building your Avalonia application:

```csharp
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseFloatingDockAdorner()
    .UseOwnerForFloatingWindows()
    .EnableWindowMagnetism()
    .SetWindowMagnetDistance(16)
    .BringWindowsToFrontOnDrag()
    .WithDockSettings(new DockSettingsOptions
    {
        MinimumHorizontalDragDistance = 6
    });
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
