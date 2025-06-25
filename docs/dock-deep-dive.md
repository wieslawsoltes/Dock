# Dock Deep Dive

This document explains the internals of `DockControl` and the docking pipeline used by Dock. It references the source code so you can trace how pointer events translate into layout changes.

## DockControl

`DockControl` is the main Avalonia control that hosts a layout. The constructor registers pointer handlers and creates both a `DockManager` and a `DockControlState` instance:

The
[constructor implementation](../src/Dock.Avalonia/Controls/DockControl.axaml.cs#L115-L126)
shows how the control registers pointer handlers and initializes the docking state.

When the `Layout` property changes `OnPropertyChanged` reinitializes the control
with the new root dock. You can inspect the logic in the
[source file](../src/Dock.Avalonia/Controls/DockControl.axaml.cs#L128-L142).

The private `Initialize` method wires up the factory and optionally calls `InitLayout`:

Initialization configures factories and optionally creates the default layout.
Refer to the
[Initialize method](../src/Dock.Avalonia/Controls/DockControl.axaml.cs#L144-L186)
for a full walkthrough.

Pointer events are delegated to `DockControlState.Process`. See
[PressedHandler and ReleasedHandler](../src/Dock.Avalonia/Controls/DockControl.axaml.cs#L253-L272)
for how the events are forwarded.

## DockControlState

`DockControlState` keeps track of the drag state. It validates potential drop targets using `DockManager` and displays `DockTarget` adorners when appropriate. Once the user releases the pointer the state object calls
[`DockManager.ValidateDockable`](../src/Dock.Avalonia/Internal/DockControlState.cs#L124-L166)
to execute the operation.

## DockManager

`DockManager` implements the algorithms that move, swap or split dockables. Methods such as `MoveDockable`, `SwapDockable` and `SplitToolDockable` call back into the factory to modify the view models.

The
[MoveDockable method](../src/Dock.Model/DockManager.cs#L18-L50)
shows how `DockManager` coordinates with the factory to move a dockable into a new dock.

## Putting it together

When the user drags a tab or tool the pointer handlers in `DockControl` delegate the event stream to `DockControlState`. The state object determines the target area and executes the appropriate operation through `DockManager`. The manager calls the factory which updates collections on the dock view models, completing the layout update.
