# Finding the Active Document

Only one dockable (document or tool) can have focus at a time, even when multiple
`IDocumentDock` instances or floating windows are present.

Use factory global tracking for cross-window scenarios:

- `IFactory.CurrentDockable`
- `IFactory.CurrentRootDock`
- `IFactory.CurrentDockWindow`
- `IFactory.GlobalDockTrackingChanged`

If you already have the root layout for a window, read `FocusedDockable` directly:

```csharp
var currentDocument = Layout?.FocusedDockable as IDocument;
```

To track focus across multiple windows, subscribe to global tracking:

```csharp
IDockable? focusedDockable = null;

factory.GlobalDockTrackingChanged += (_, e) =>
{
    focusedDockable = e.Current.Dockable;
    Console.WriteLine($"Focused dockable: {e.Current.Dockable?.Title}");
};

var currentDocument = focusedDockable as IDocument;
```

If you need the owning root/window:

```csharp
var focusedRoot = factory.CurrentRootDock;
var focusedWindow = factory.CurrentDockWindow;
```

You can also use helper extensions:

```csharp
var currentDocument = factory.GetCurrentDocument();
var activeRoot = factory.GetActiveRoot();
```

```csharp
// Close whichever dockable currently has focus.
if (focusedDockable is { } dockable)
{
    factory.CloseDockable(dockable);
}
```

The focused dockable can be `null` when nothing is focused.
