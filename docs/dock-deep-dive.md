# Dock Deep Dive

This document explains how `DockControl` routes pointer input through the docking pipeline. Reading this guide together with the source code will help you understand how layout changes are performed.

While the high level guides focus on using the provided factories this section
dives into the low level mechanics. Understanding these classes is useful when
you need behaviour not covered by the default implementation.

## DockControl

`DockControl` is the main Avalonia control that hosts a layout. Its constructor registers pointer handlers and by default creates a `DockManager` and `DockControlState`. You can provide custom instances via the constructor or by assigning the properties:

```csharp
var manager = new CustomDockManager();
var state = new DockControlState(manager, new DefaultDragOffsetCalculator());
var dockControl = new DockControl(manager, state);
```

When the `Layout` property changes the control is reinitialised with the new root dock:

```csharp
protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
{
    if (e.Property == LayoutProperty)
        Initialize(e.NewValue as IDock);
    base.OnPropertyChanged(e);
}
```

The private `Initialize` method wires up the factory and optionally calls `InitLayout`:

```csharp
private void Initialize(IDock? layout)
{
    if (layout is null)
        return;

    Layout = layout;
    Layout.Factory?.InitLayout(layout);
}
```

Pointer events are forwarded to `DockControlState.Process` which performs hit testing and drag logic:

```csharp
private void PressedHandler(object? sender, PointerPressedEventArgs e)
{
    var position = e.GetPosition(this);
    _dockControlState.Process(position, Vector.Zero, EventType.Pressed, ToDragAction(e), this, Layout?.Factory?.DockControls);
}
```

## DockControlState

`DockControlState` keeps track of the current drag operation. It validates potential drop targets using `DockManager` and displays dock target controls derived from `DockTargetBase` (for example `DockTarget` and `GlobalDockTarget`) when appropriate. Once the user releases the pointer the state calls `DockManager.ValidateDockable` with execution enabled.

## DockManager

`DockManager` implements the algorithms that move, swap or split dockables. Methods such as `MoveDockable`, `SwapDockable` and `SplitToolDockable` call back into the factory to modify the view models.

```csharp
// Example: move a document
_dockManager.MoveDockable(document, targetDock, index);
```

## Putting it together

When the user drags a tab or tool the pointer handlers in `DockControl` delegate the event stream to `DockControlState`. The state object determines the target area and executes the appropriate operation through `DockManager`. The manager calls the factory which updates collections on the dock view models, completing the layout update.

If you wish to extend this pipeline—for example to implement custom drag
restrictions—you can subclass `DockManager` and override its methods before
assigning it to the `DockControl` instance.

For an overview of all guides see the [documentation index](README.md).
