# Finding the Active Document

Only one dockable (document or tool) can have focus at a time, even when multiple
`IDocumentDock` instances or floating windows are present. The root dock that owns
the focused dockable stores it in `FocusedDockable`, and the factory reports focus
changes through `IFactory.FocusedDockableChanged`.

If you already have the root layout for a window, read `FocusedDockable` directly:

```csharp
var currentDocument = Layout?.FocusedDockable as IDocument;
```

To track focus across multiple windows, subscribe to the factory event and cache
the latest dockable:

```csharp
IDockable? focusedDockable = null;

factory.FocusedDockableChanged += (_, e) =>
{
    focusedDockable = e.Dockable;
    Console.WriteLine($"Focused dockable: {e.Dockable?.Title}");
};

var currentDocument = focusedDockable as IDocument;
```

If you need the root dock that owns the focused dockable:

```csharp
var focusedRoot = focusedDockable is { } dockable
    ? factory.FindRoot(dockable, root => root.IsFocusableRoot)
    : null;
```

```csharp
// Close whichever dockable currently has focus.
if (focusedDockable is { } dockable)
{
    factory.CloseDockable(dockable);
}
```

The focused dockable can be `null` when nothing is focused. In multi-window
layouts, each root keeps its last focused dockable, so use the event to determine
the current focus.
