# DockManager Usage Guide

`DockManager` implements the core algorithms that move, swap and split dockables. It is created by `DockControl` but you can also instantiate it manually for custom hosts.

## Why use DockManager?

- The manager centralises all drag-and-drop logic, keeping `DockControl` free from layout code.
- It exposes the `PreventSizeConflicts` property which blocks two fixed-size tools from being docked together.
- The manager calls back into the factory so your view models are updated consistently.

## Basic usage

`DockControl` automatically creates a `DockManager` when constructed. You can access it via the read-only `DockControl.DockManager` property if you need to inspect settings like `PreventSizeConflicts`.

If you are building a custom host (not using `DockControl`), create your own `DockManager` instance:

```csharp
var dockManager = new DockManager(new DockService())
{
    PreventSizeConflicts = true
};
```

During a drag operation `DockControlState` calls `DockManager.ValidateDockable` to test potential drop targets. When the pointer is released the same call is executed with `bExecute: true` so the layout is updated.

## Customizing behavior

`DockManager` implements `IDockManager` but does not expose virtual methods. If you need custom docking rules you must implement `IDockManager` yourself (optionally delegating to `DockManager`) and use that in a custom host control or a forked `DockControl`. Common customizations include:

- Disallowing certain dock operations, e.g. preventing documents from being split vertically.
- Changing the logic used when floating dockables into windows.
- Recording diagnostics about drag and drop events.

Example wrapping `DockManager` to forbid tools from floating:

```csharp
public sealed class CustomDockManager : IDockManager
{
    private readonly DockManager _inner = new(new DockService());

    public DockPoint Position { get => _inner.Position; set => _inner.Position = value; }
    public DockPoint ScreenPosition { get => _inner.ScreenPosition; set => _inner.ScreenPosition = value; }
    public bool PreventSizeConflicts { get => _inner.PreventSizeConflicts; set => _inner.PreventSizeConflicts = value; }

    public bool ValidateTool(ITool sourceTool, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Window)
            return false;
        return _inner.ValidateTool(sourceTool, target, action, operation, execute);
    }

    public bool ValidateDocument(IDocument sourceDocument, IDockable target, DragAction action, DockOperation operation, bool execute) =>
        _inner.ValidateDocument(sourceDocument, target, action, operation, execute);

    public bool ValidateDock(IDock sourceDock, IDockable target, DragAction action, DockOperation operation, bool execute) =>
        _inner.ValidateDock(sourceDock, target, action, operation, execute);

    public bool ValidateDockable(IDockable sourceDockable, IDockable target, DragAction action, DockOperation operation, bool execute) =>
        _inner.ValidateDockable(sourceDockable, target, action, operation, execute);

    public bool IsDockTargetVisible(IDockable sourceDockable, IDockable targetDockable, DockOperation operation) =>
        _inner.IsDockTargetVisible(sourceDockable, targetDockable, operation);
}
```

Use your `IDockManager` implementation in a custom host control. `DockControl` always constructs its own `DockManager`.

## Guidelines for custom managers

1. **Keep factory calls intact.** Most methods delegate changes to the factory. If you override them ensure you still call the factory so view models remain in sync.
2. **Check `PreventSizeConflicts`.** When splitting tools always respect this flag or provide an alternative mechanism to avoid invalid layouts.
3. **Validate before executing.** The built-in state first validates with `bExecute: false` then re-validates with `bExecute: true` on drop. Your overrides should follow this pattern to avoid inconsistent states. The validation also considers docking groups - see [Docking Groups](dock-docking-groups.md) for details on how this restriction system works.
4. **Consider user experience.** Changes to docking rules can dramatically affect how the UI feels. Provide visual feedback if an action is disallowed.

The manager is a small but critical part of Dock. By tailoring it you can adapt the docking behavior to suit almost any workflow.

## Dock target visibility

`DockManager` exposes `IsDockTargetVisible` which determines whether a drop
indicator is shown. The default implementation hides the center indicator when
the dragged dockable equals the potential target or when the potential target is
the current owner of the dragged dockable. Override this method to apply your
own rules.

For an overview of other concepts see the [documentation index](README.md).
