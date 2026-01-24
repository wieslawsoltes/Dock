# Managed Windows Guide

Managed windows host floating docks inside the main window instead of spawning native OS windows. When enabled, Dock uses the in-app MDI layer to render floating windows as `MdiDocumentWindow` controls.

## When to use managed windows

Managed windows are useful when:

- Your app should stay inside a single top-level window.
- You want consistent window chrome across platforms.
- Native window restrictions or airspace issues make OS windows undesirable.

The main window is still an OS window. Managed windows affect only floating dock windows.

## How managed windows work

1. `DockSettings.UseManagedWindows` switches floating windows to `ManagedHostWindow`.
2. `DockControl` registers its `ManagedWindowLayer` with the factory when `EnableManagedWindowLayer` is true.
3. `ManagedHostWindow` creates a `ManagedDockWindowDocument` that wraps the `IDockWindow` model.
4. `ManagedWindowLayer` renders managed windows using `MdiLayoutPanel` and `MdiDocumentWindow`.

The managed windows remain in the main visual tree so they can share styles and resources with the rest of the app.

## Interaction parity

Managed windows reuse the same interaction model as native windows:

- Dragging and resizing are handled by `MdiDocumentWindow` and its layout manager.
- Dock targets, drag previews, and pinned windows are rendered inside the managed layer.
- Window magnetism and bring-to-front behavior apply within the managed layer.

## Differences from native windows

Managed windows are not OS windows:

- They do not appear in the taskbar or window switchers.
- They cannot be moved outside the main window or across monitors.
- Owner relationships and OS-level z-order do not apply; ordering uses `MdiZIndex`.

## Theming and templates

`ManagedWindowLayer` is part of the `DockControl` template and must be named `PART_ManagedWindowLayer`. If you replace the template, include the layer and keep `EnableManagedWindowLayer` set to `true` so floating windows remain visible.

## Related articles

- [Managed windows how-to](dock-managed-windows-howto.md)
- [Managed windows reference](dock-managed-windows-reference.md)
- [Floating windows](dock-windows.md)
- [MDI document layout](dock-mdi.md)
