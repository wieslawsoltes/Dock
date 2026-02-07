# Global Dock Tracking

This document defines the global tracking model introduced for multi-window focus and active document resolution.

## Problem

Per-dock events (`ActiveDockableChanged`, `FocusedDockableChanged`) previously exposed only a dockable reference. In multi-window layouts this made it hard to reliably identify:

- which root dock currently owns focus
- which floating dock window is currently active
- where global overlays (busy/dialog) should render

## Expected behavior

Global tracking should always resolve the current dock context across all windows:

- Current dockable (document/tool)
- Current root dock
- Current dock window (when floating)
- Current host window

## API

`IFactory` now exposes global state:

- `GlobalDockTrackingState GlobalDockTrackingState`
- `IDockable? CurrentDockable`
- `IRootDock? CurrentRootDock`
- `IDockWindow? CurrentDockWindow`
- `IHostWindow? CurrentHostWindow`

`IFactory` now exposes a global state event:

- `GlobalDockTrackingChanged`

`ActiveDockableChangedEventArgs` and `FocusedDockableChangedEventArgs` now include:

- `RootDock`
- `Window`
- `HostWindow`

## Tracking rules

State is updated from window/focus/activation transitions:

1. `FocusedDockableChanged` updates global state to the focused dockable context.
2. `WindowActivated` updates global state to that window plus its focused/active dockable.
3. `DockableActivated` updates global state only when the activation belongs to the tracked root (or no root is tracked yet).
4. `WindowDeactivated` clears state when the deactivated window is the tracked window.
5. `WindowClosed` and `WindowRemoved` clear state when they affect the tracked window.

`ActiveDockableChanged` and `DockableActivated` still raise normally, but global state only updates when the change belongs to the already tracked root (or no root is tracked yet). This avoids background roots overriding global context.

## Active document helpers

`FactoryExtensions` now uses global tracking first:

- `GetActiveRoot()` -> `CurrentRootDock` fallback to legacy `IsActive` scan.
- `GetCurrentDocument()` -> `CurrentDockable` fallback to focused dockable on active root.
- `CloseFocusedDockable()` closes `CurrentDockable` first.

## Overlay host resolution improvements

`IHostOverlayServicesProvider` now supports dockable-aware resolution:

```csharp
IHostOverlayServices GetServices(IScreen screen, IDockable dockable);
```

This allows busy/dialog/confirmation overlays to resolve by the dockable's current root/window, with `screen` used as fallback.

`RoutableDocumentBase` and `RoutableToolBase` use this dockable-aware overload by default.

## Sample status bar

`DockMvvmSample` and `DockReactiveUISample` include a status bar bound to global tracking state and updated through `GlobalDockTrackingChanged`.
