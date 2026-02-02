# Window Ownership

Dock supports per-window ownership modes and a global owner policy. Use these to control z-order, minimize/restore linkage, and modal dialog parenting.

## Per-Window Owner Modes
Set `IDockWindow.OwnerMode` or `DockWindowOptions.OwnerMode`:
- `Default` uses `ParentWindow` if provided, otherwise falls back to global owner policy.
- `None` never assigns an owner.
- `ParentWindow` uses `ParentWindow` explicitly.
- `DockableWindow` uses the window that currently hosts the dockable being floated.
- `RootWindow` uses the root dock window.

### Example
```csharp
var options = new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.DockableWindow
};

factory.FloatDockable(tool, options);
```

## Global Owner Policy
`DockSettings.FloatingWindowOwnerPolicy` controls the default owner behavior when `OwnerMode` is `Default`:
- `Default` defers to `DockSettings.UseOwnerForFloatingWindows`.
- `AlwaysOwned` always assigns an owner when possible.
- `NeverOwned` never assigns an owner.

### Example
```csharp
DockSettings.FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.NeverOwned;
```

## Compatibility
`DockSettings.UseOwnerForFloatingWindows` is still supported and remains the default. When the owner policy is `Default`, Dock uses this legacy flag to keep existing behavior unchanged.
