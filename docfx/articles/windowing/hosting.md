# Host Modes (Managed vs Native)

Dock can host floating windows as native OS windows or as managed in-app windows.

## Host Mode Resolution
The effective host mode is resolved in order:
1. `IRootDock.FloatingWindowHostMode` (if not `Default`).
2. `DockSettings.FloatingWindowHostMode` (if not `Default`).
3. `DockSettings.UseManagedWindows` (legacy default).

`Managed` uses `ManagedHostWindow` (in-app). `Native` uses `HostWindow` (OS window).

## Per-Root Override
Use `IRootDock.FloatingWindowHostMode` to override host mode for a specific root dock.

```csharp
root.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
```

## Global Setting
```csharp
DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;
```

## Compatibility
`DockSettings.UseManagedWindows` remains supported and is used when both the root and global host mode are `Default`.
