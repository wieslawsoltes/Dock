# Programmatic Docking with DockService

`DockService` is the low-level implementation of `IDockService` used by `DockManager` to validate and execute docking operations. Use it when you are building a custom host, testing docking logic, or triggering dock moves from code while keeping the same rules as drag-and-drop.

## When to use DockService

- You need the same validation logic that the drag-and-drop pipeline uses.
- You are hosting Dock without `DockControl` and still want docking behavior.
- You want to test docking rules (including docking groups) without UI interaction.

For most app-level operations, prefer the factory helpers (`AddDockable`, `MoveDockable`, `FloatDockable`, etc.). `DockService` sits below those APIs and assumes owners and factories are already initialized.

## Validation pattern

All methods take a `bExecute` flag. Call with `false` to validate, then with `true` to apply:

```csharp
using Dock.Model;
using Dock.Model.Core;

var dockService = new DockService();

if (dockService.MoveDockable(source, sourceOwner, targetDock, false))
{
    dockService.MoveDockable(source, sourceOwner, targetDock, true);
}
```

This matches the workflow in `DockManager`, which validates a target during drag and executes on drop.

## Operations overview

### MoveDockable

Moves a dockable into a target dock. For empty target docks, `DockService` uses global docking validation so ungrouped dockables can dock anywhere. For populated docks it enforces strict docking group compatibility across the entire target dock.

### SwapDockable

Swaps the source with the target dock's active dockable (or the last visible dockable if none is active). Docking group compatibility is enforced for the entire target dock.

### SplitDockable

Splits the target dock into a new dock created by the factory:

- `ITool` sources create a new `IToolDock`.
- `IDocument` sources create a new `IDocumentDock`.
- Document docks copy `Id`, `CanCreateDocument`, `EnableWindowDrag`, `DocumentTemplate` (when the dock implements `IDocumentDockContent`), and `DocumentFactory` (when the dock implements `IDocumentDockFactory`).

The split uses the specified `DockOperation` (Left, Right, Top, Bottom). Group compatibility is validated against the new empty dock before the split is applied.

### DockDockableIntoDockable

Moves or swaps dockables based on the `DragAction`:

- `DragAction.Move` docks into the target.
- `DragAction.Link` swaps with the target.
- `DragAction.Copy` is not supported and returns `false`.

This method handles both local docking (same owner) and cross-dock docking.

### DockDockableIntoWindow

Floats a dockable into a new window if `CanFloat` is `true`. The service uses the visible bounds of the dockable or its owner to size the floating window, falling back to default dimensions when no bounds are available. The target window is created through the factory's `SplitToWindow` method.

## Requirements and tips

- Ensure `Owner` and `Factory` are set on dockables by calling `InitLayout` or `InitDockable` from your factory.
- Docking group rules are enforced by `DockGroupValidator`. See [Docking groups](dock-docking-groups.md) for details.
- If you need to customize docking rules, wrap or replace `IDockService` when constructing your own `DockManager`.

For an overview of other guides see the [documentation index](README.md).
