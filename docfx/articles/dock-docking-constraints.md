# Docking Constraints and Layout Lock

Dock provides two levels of guardrails to match professional workflows: a global layout lock toggle and per-dockable docking restrictions.

## Layout lock

Use `DockControl.IsDockingEnabled` (or `IDockManager.IsDockingEnabled`) to disable drag and docking operations across a layout. This mirrors the "Lock UI" feature in IDEs and CAD tools.

```csharp
// Disable docking interactions
myDockControl.IsDockingEnabled = false;

// Re-enable when needed
myDockControl.IsDockingEnabled = true;
```

If you create custom floating windows, pass the same `DockManagerOptions` instance so the lock applies everywhere:

```csharp
var options = myDockControl.DockManagerOptions;
myFactory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
{
    [nameof(IDockWindow)] = () => new HostWindow(options)
};
```

## Per-dockable docking restrictions

Dockables can opt into `IDockableDockingRestrictions` to control where they can be docked.

- `AllowedDockOperations` applies when the dockable is the drag source.
- `AllowedDropOperations` applies when the dockable is the drop target.

Both properties use the `DockOperationMask` flags enum.

```csharp
// Only allow document tabs, no split operations
documentDock.AllowedDropOperations = DockOperationMask.Fill;

// Tool can only dock on the left or float
tool.AllowedDockOperations = DockOperationMask.Left | DockOperationMask.Fill | DockOperationMask.Window;
```

These constraints affect validation and the docking indicators, so disallowed operations are hidden and rejected.

