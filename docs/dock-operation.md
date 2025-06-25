# Dock Operation Guide

Dock uses the `DockOperation` enumeration to describe how a dockable should be positioned relative to another dock or window. Runtime helpers such as `SplitDockable` and `DockDockable` accept one of these values when rearranging the layout.

## Basic operations

- `Fill` – Replace the contents of the target dock.
- `Left` – Place the dockable to the left side.
- `Right` – Place the dockable to the right side.
- `Top` – Place the dockable above.
- `Bottom` – Place the dockable below.
- `Window` – Open the dockable in a separate window.

## Diagonal operations

In addition to the basic directions, Dock supports corner docking. The diagonal values map to the same split logic but allow selecting the exact corner in the UI:

- `TopLeft`
- `TopRight`
- `BottomLeft`
- `BottomRight`

These can be chosen in the `DockTarget` overlay when dragging a dockable.

For an overview of all guides see the [documentation index](README.md).
