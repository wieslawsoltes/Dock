# Drag Actions and Modifiers

Dock uses `DragAction` to decide what to do when you drop a dockable. This applies to drag operations started from tab strips, pinned tabs, and general drag gestures in `DockControl`.

## Modifier keys

The default mapping is implemented in `DockControl`, `DocumentTabStripItem`, `ToolTabStripItem`, and `ToolPinItemControl`:

| Modifier | DragAction | Behavior |
| --- | --- | --- |
| `Alt` | `Link` | Swap with the target dockable. |
| `Ctrl` | `Copy` | Copy (not supported by the default manager). |
| `Shift` | `Move` | Move into the target dock. |
| none | `Move` | Move into the target dock. |

`Shift` uses the same behavior as the default (move), so the only behavioral changes are `Alt` (swap) and `Ctrl` (copy).

## How actions are applied

`DockManager` interprets `DragAction` during validation and execution:

- `Move` docks into the target (`MoveDockable`/`SplitDockable`/`DockDockableIntoWindow`).
- `Link` swaps the source and target (`SwapDockable`).
- `Copy` returns `false` by default (no built-in copy implementation).

If you need copy semantics, implement a custom docking pipeline that clones your dockable and inserts it manually.

## Programmatic docking

The same actions are used when calling into `DockManager` or `DockService`. See
[Programmatic docking](dock-programmatic-docking.md) for API examples.

For related enums see [Enumerations](dock-enums.md).
