# Dock Settings

`Dock.Settings` exposes global properties and constants that control drag and drop behavior.
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
| `ShowDockIndicatorOnly` | `bool` | Shows only drop indicators, hiding the dock target visuals. |
| `IndicatorDockOperation` | `DockOperation` | Sets the dock operation when showing indicators only. |
| `DockAdornerHost` | `Control` | Hosts the dock target adorner instead of the adorned control. |
| `DockGroup` | `string` | Restricts docking to matching groups (inherited). |

Use `DockGroup` to restrict docking operations across your layout. For details see [Docking groups](dock-docking-groups.md).

Example disabling drag and drop for an entire window:

```xaml
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

`DockSettings.GlobalDockingProportion` controls the split ratio used when a dockable
is dropped as a global dock target (for example, dropping into a different window).
The default value is `0.5`.

`DockSettings.GlobalDockingPreset` controls how global docking decisions are made.
The default value is `DockGlobalDockingPreset.GlobalFirst`.

- `DockGlobalDockingPreset.LocalFirst`:
  - Uses legacy local-first operation selection.
  - Resolves global target from the immediate drop context.
- `DockGlobalDockingPreset.GlobalFirst`:
  - Uses global-first operation selection when a global target is active.
  - Resolves global target to the outermost global target in the owner chain.

Example:

```csharp
DockSettings.GlobalDockingPreset = DockGlobalDockingPreset.GlobalFirst;
DockSettings.GlobalDockingProportion = 0.33;
```

Set these before docking interactions start (typically during app startup).

## Floating window owner

`DockSettings.UseOwnerForFloatingWindows` keeps floating windows above the main window by setting it as their owner. This is applied when `IDockWindow.OwnerMode` is `DockWindowOwnerMode.Default` and can be overridden per window.

`DockSettings.FloatingWindowOwnerPolicy` provides an explicit global owner policy (`Default`, `AlwaysOwned`, `NeverOwned`). When set to `Default`, Dock falls back to `UseOwnerForFloatingWindows` to preserve compatibility.

## Floating window host mode

`DockSettings.FloatingWindowHostMode` controls whether floating windows are hosted as native OS windows or managed in-app windows. `Default` defers to `DockSettings.UseManagedWindows`.

## Floating dock adorners

`DockSettings.UseFloatingDockAdorner` enables showing drop indicators in a transparent floating window instead of overlays on the target controls. See [Floating Dock Adorners](dock-floating-adorners.md) for details.

## Pinned dock windows

`DockSettings.UsePinnedDockWindow` shows auto-hidden dockables inside a floating window instead of sliding panels. See [Pinned Dock Window](dock-pinned-window.md) for details.

## Managed windows

`DockSettings.UseManagedWindows` hosts floating windows inside the main window using the managed window layer. This affects only floating docks; the main window remains native. See [Managed windows guide](dock-managed-windows-guide.md) for details.

`IRootDock.FloatingWindowHostMode` can override the host mode for a specific root dock.

## Window magnetism

`DockSettings.EnableWindowMagnetism` toggles snapping of floating windows. The snap distance
is controlled by `DockSettings.WindowMagnetDistance`.

## Bring windows to front on drag

`DockSettings.BringWindowsToFrontOnDrag` determines whether all floating windows
and any main window hosting a `DockControl` are activated when dragging begins.
Enabled by default.

## Drag preview

`DockSettings.ShowDockablePreviewOnDrag` toggles whether the drag preview window
renders the full dockable layout instead of only the title/status badge.

`DockSettings.DragPreviewOpacity` controls the preview window opacity (0.0 to 1.0).

## ItemsSource unregister synchronization

`DockSettings.UpdateItemsSourceOnUnregister` controls whether closing an ItemsSource-generated
document/tool attempts to remove the source item from the backing collection (when it implements `IList`).
The default is `true`.

Per-dock overrides are available through:

- `DocumentDock.CanUpdateItemsSourceOnUnregister`
- `ToolDock.CanUpdateItemsSourceOnUnregister`

When a per-dock value is `null`, Dock uses the global `DockSettings` value.

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

See [Selector overlay](dock-selector-overlay.md) for behavior and navigation details.

## Command bar merging

Command bar merging is disabled by default. Enable it and choose a scope:

```csharp
DockSettings.CommandBarMergingEnabled = true;
DockSettings.CommandBarMergingScope = DockCommandBarMergingScope.ActiveDocument;
```

See [Command bars](dock-command-bars.md) for definitions and merge behavior.

## Diagnostics logging

Enable verbose diagnostics logging when you need to inspect docking workflows:

```csharp
DockSettings.EnableDiagnosticsLogging = true;
DockSettings.DiagnosticsLogHandler = message => Debug.WriteLine(message);
```

When enabled, Dock writes internal messages to `DiagnosticsLogHandler` if provided,
otherwise it uses standard debug output.

You can also emit your own messages through `DockLogger`. For details and examples
see [Diagnostics logging](dock-diagnostics-logging.md).

## App builder integration

You can configure the settings when building your Avalonia application:

```csharp
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseFloatingDockAdorner()
    .UseOwnerForFloatingWindows()
    .UseManagedWindows()
    .EnableWindowMagnetism()
    .SetWindowMagnetDistance(16)
    .BringWindowsToFrontOnDrag()
    .ShowDockablePreviewOnDrag()
    .SetDragPreviewOpacity(0.6)
    .UpdateItemsSourceOnUnregister(true)
    .WithDockSettings(new DockSettingsOptions
    {
        MinimumHorizontalDragDistance = 6
    });
```

## DockSettingsOptions reference

`DockSettingsOptions` mirrors `DockSettings` and lets you override individual values when calling `WithDockSettings`. Any property left as `null` keeps the current `DockSettings` value.

| Option | Applies to | Description |
| --- | --- | --- |
| `MinimumHorizontalDragDistance` | `DockSettings.MinimumHorizontalDragDistance` | Horizontal drag threshold. |
| `MinimumVerticalDragDistance` | `DockSettings.MinimumVerticalDragDistance` | Vertical drag threshold. |
| `UseFloatingDockAdorner` | `DockSettings.UseFloatingDockAdorner` | Floating adorner window. |
| `UsePinnedDockWindow` | `DockSettings.UsePinnedDockWindow` | Floating pinned dock window. |
| `UseManagedWindows` | `DockSettings.UseManagedWindows` | Managed floating windows. |
| `UseOwnerForFloatingWindows` | `DockSettings.UseOwnerForFloatingWindows` | Assign owners to floating windows. |
| `EnableWindowMagnetism` | `DockSettings.EnableWindowMagnetism` | Snap floating windows. |
| `WindowMagnetDistance` | `DockSettings.WindowMagnetDistance` | Snap distance in pixels. |
| `BringWindowsToFrontOnDrag` | `DockSettings.BringWindowsToFrontOnDrag` | Activate windows when dragging. |
| `ShowDockablePreviewOnDrag` | `DockSettings.ShowDockablePreviewOnDrag` | Render dockable content in drag preview. |
| `DragPreviewOpacity` | `DockSettings.DragPreviewOpacity` | Opacity for the drag preview window. |
| `UpdateItemsSourceOnUnregister` | `DockSettings.UpdateItemsSourceOnUnregister` | Sync close/unregister of generated items back to source collections. |
| `SelectorEnabled` | `DockSettings.SelectorEnabled` | Toggle selector overlay. |
| `DocumentSelectorKeyGesture` | `DockSettings.DocumentSelectorKeyGesture` | Document selector shortcut. |
| `ToolSelectorKeyGesture` | `DockSettings.ToolSelectorKeyGesture` | Tool selector shortcut. |
| `CommandBarMergingEnabled` | `DockSettings.CommandBarMergingEnabled` | Enable command bar merging. |
| `CommandBarMergingScope` | `DockSettings.CommandBarMergingScope` | Merge scope for command bars. |

`GlobalDockingPreset` is currently configured directly on `DockSettings`
rather than through `DockSettingsOptions`.

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
