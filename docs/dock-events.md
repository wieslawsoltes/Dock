# Dock Events Guide

Dock exposes a large set of runtime events through `FactoryBase` so that
applications can react to changes in the layout.  The existing guides only
briefly mention these hooks.  This document lists the most commonly used events
and shows how to subscribe to them.

## Common events

`FactoryBase` publishes events via the `IFactory` interface.  Each event passes
an arguments object containing the affected dockable or window.

| Event | Description |
| ----- | ----------- |
| `ActiveDockableChanged` | Fired when the active dockable within a dock changes. |
| `FocusedDockableChanged` | Triggered when focus moves to another dockable. |
| `DockableAdded` | Raised after a dockable is inserted into a dock. |
| `DockableRemoved` | Raised after a dockable has been removed. |
| `DockableClosed` | Occurs when a dockable is closed via command or UI. |
| `DockableMoved` | Indicates a dockable was rearranged within its parent. |
| `DockablePinned` / `DockableUnpinned` | Signalled when a tool is pinned or unpinned. |
| `WindowOpened` / `WindowClosed` | Fired when a floating window is created or closed. |

Other specialized events like `WindowMoveDragBegin`, `WindowMoveDrag` and
`WindowMoveDragEnd` allow intercepting drag operations.

## Subscribing to events

Create a factory instance and attach handlers before initializing the layout:

```csharp
var factory = new DockFactory();
factory.ActiveDockableChanged += (_, args) =>
    Debug.WriteLine($"Active dockable: {args.Dockable?.Title}");
factory.WindowOpened += (_, args) =>
    Debug.WriteLine($"Opened window: {args.Window?.Title}");

var layout = factory.CreateLayout();
factory.InitLayout(layout);
```

Each event provides enough information to update the view model or perform
logging.  Cancelling operations is possible for some events by setting the
`Cancel` property on the event args instance.

## When to use events

Events are useful for:

- Tracking which document or tool currently has focus.
- Persisting window placement after the user moves a floating window.
- Implementing custom logic when new documents are created at runtime.

Consult the samples for practical examples where events are wired to view models
and commands.
