# Dock Deep Dive

This document explains the internals of `DockControl` and the docking pipeline used by Dock. It references the source code so you can trace how pointer events translate into layout changes.

## DockControl

`DockControl` is the main Avalonia control that hosts a layout. The constructor registers pointer handlers and creates both a `DockManager` and a `DockControlState` instance:

```csharp
// DockControl.axaml.cs
```

When the `Layout` property changes `OnPropertyChanged` reinitializes the control with the new root dock:

```csharp
// DockControl.axaml.cs
```

The private `Initialize` method wires up the factory and optionally calls `InitLayout`:

```csharp
// DockControl.axaml.cs
```

Pointer events are forwarded to `DockControlState.Process` which performs hit testing and drag logic:

```csharp
// DockControl.axaml.cs
```

## DockControlState

`DockControlState` keeps track of the drag state. It validates potential drop targets using `DockManager` and displays `DockTarget` adorners when appropriate. Once the user releases the pointer the state object calls `DockManager.ValidateDockable` with execution enabled.

```csharp
// DockControlState.cs
```

## DockManager

`DockManager` implements the algorithms that move, swap or split dockables. Methods such as `MoveDockable`, `SwapDockable` and `SplitToolDockable` call back into the factory to modify the view models.

```csharp
// DockManager.cs
```

## Putting it together

When the user drags a tab or tool the pointer handlers in `DockControl` delegate the event stream to `DockControlState`. The state object determines the target area and executes the appropriate operation through `DockManager`. The manager calls the factory which updates collections on the dock view models, completing the layout update.
