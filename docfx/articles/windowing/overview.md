# Windowing Overview

Dock uses `IDockWindow` to represent floating windows and a host window (`IHostWindow`) to present them. The default behavior stays the same as before: floating windows are owned by the main window and use native OS windows unless you change settings.

## Key Concepts
- `IDockWindow` is the model for a floating window (title, bounds, owner, modality, taskbar visibility).
- `IHostWindow` is the platform host that presents the window (native or managed).
- `DockWindowOptions` lets you configure ownership, modality, and taskbar visibility when creating windows.
- `IRootDock` can override the host mode for the windows it owns.

## Defaults
- Floating windows are **owned** by the main window when `DockSettings.UseOwnerForFloatingWindows = true`.
- Floating windows are **native** when `DockSettings.UseManagedWindows = false`.
- Modal windows require a resolved owner; if none can be found they are shown non-modally with diagnostics logging enabled.

## Related Articles
- Ownership and owner policies: `windowing/ownership.md`
- Host mode (managed vs native): `windowing/hosting.md`
- Taskbar visibility and modality: `windowing/taskbar-and-modal.md`
