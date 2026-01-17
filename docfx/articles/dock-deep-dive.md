# Dock Deep Dive

This document explains how `DockControl` routes pointer input through the docking pipeline. Reading this guide together with the source code will help you understand how layout changes are performed.

While the high level guides focus on using the provided factories this section
dives into the low level mechanics. Understanding these classes is useful when
you need behavior not covered by the default implementation.

## DockControl

`DockControl` is the main Avalonia control that hosts a layout. Its constructor creates a `DockManager` (backed by `DockService`), a `DockControlState`, and registers pointer handlers for press, release, move, enter/exit, capture loss, wheel, and keyboard events.

When the `Layout` property changes, the control deinitializes the previous layout (if any) and initializes the new one. Initialization ensures a factory is assigned, registers the `DockControl` with that factory, optionally sets default locators when `InitializeFactory` is `true`, and optionally calls `InitLayout` when `InitializeLayout` is `true`.

Pointer events are forwarded to `DockControlState.Process`, which performs hit testing, drag/drop state updates, and docking execution using the current set of dock controls.

## DockControlState

`DockControlState` keeps track of the current drag operation. It validates potential drop targets using `DockManager` and displays dock target controls derived from `DockTargetBase` (for example `DockTarget` and `GlobalDockTarget`) when appropriate. Once the user releases the pointer, the state executes the docking operation through `DockManager`.

## DockManager

`DockManager` implements the algorithms that move, swap or split dockables. Methods such as `MoveDockable`, `SwapDockable` and `SplitToolDockable` call back into the factory to modify the view models.

```csharp
// Example: move a document
_dockManager.MoveDockable(document, targetDock, index);
```

## Putting it together

When the user drags a tab or tool the pointer handlers in `DockControl` delegate the event stream to `DockControlState`. The state object determines the target area and executes the appropriate operation through `DockManager`. The manager calls the factory which updates collections on the dock view models, completing the layout update.

If you wish to extend this pipeline—for example to implement custom drag
restrictions—note that `DockControl` always constructs its own `DockManager`.
To customize behavior, implement `IDockManager` (optionally delegating to
`DockManager`) in a custom host control or a forked `DockControl`.

For an overview of all guides see the [documentation index](README.md).
