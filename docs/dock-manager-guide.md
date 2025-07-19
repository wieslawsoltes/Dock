# DockManager Usage Guide

`DockManager` implements the core algorithms that move, swap and split dockables. It is created by `DockControl` but you can also instantiate it manually for custom hosts.

## Why use DockManager?

- The manager centralises all drag-and-drop logic, keeping `DockControl` free from layout code.
- The manager is configured via `DockManagerOptions` which includes the `PreventSizeConflicts` flag to block docking tools with incompatible fixed sizes.
- The manager calls back into the factory so your view models are updated consistently.

## Basic usage

`DockControl` automatically creates a `DockManager` with default options. To customise the behaviour pass an options object to the control constructor:

```csharp
var options = new DockManagerOptions
{
    PreventSizeConflicts = false
};
var dockControl = new DockControl(options)
{
    Layout = factory.CreateLayout()
};
```

During a drag operation `DockControlState` calls `DockManager.ValidateDockable` to test potential drop targets. When the pointer is released the same call is executed with `bExecute: true` so the layout is updated.

## Overriding behaviour

`DockManager` is a regular class so you can inherit from it and override any method. This is useful when the standard rules do not match your application's requirements. Common customisations include:

- Disallowing certain dock operations, e.g. preventing documents from being split vertically.
- Changing the logic used when floating dockables into windows.
- Recording diagnostics about drag and drop events.

Example overriding `ValidateTool` to forbid tools from floating:

```csharp
public class CustomDockManager : DockManager
{
    public override bool ValidateTool(ITool sourceTool, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Window)
            return false;
        return base.ValidateTool(sourceTool, target, action, operation, execute);
    }
}
```

Assign the subclass to the control as shown earlier. All other methods will continue to use the default logic.

## Guidelines for custom managers

1. **Keep factory calls intact.** Most methods delegate changes to the factory. If you override them ensure you still call the factory so view models remain in sync.
2. **Check `PreventSizeConflicts`.** When splitting tools always respect this flag or provide an alternative mechanism to avoid invalid layouts.
3. **Validate before executing.** The built-in state first validates with `bExecute: false` then re-validates with `bExecute: true` on drop. Your overrides should follow this pattern to avoid inconsistent states.
4. **Consider user experience.** Changes to docking rules can dramatically affect how the UI feels. Provide visual feedback if an action is disallowed.

The manager is a small but critical part of Dock. By tailoring it you can adapt the docking behaviour to suit almost any workflow.

## Dock target visibility

`DockManager` exposes `IsDockTargetVisible` which determines whether a drop
indicator is shown. The default implementation hides the center indicator when
the dragged dockable equals the potential target or when the potential target is
the current owner of the dragged dockable. Override this method to apply your
own rules.

For an overview of other concepts see the [documentation index](README.md).
