# Finding the Active Document

Only one document or tool can have focus at a time, even when multiple
`IDocumentDock` instances or floating windows are present.  The
`IRootDock` that is currently active exposes its focused dockable via the
`FocusedDockable` property.  You can subscribe to the
`IFactory.FocusedDockableChanged` event or query the property directly.

```csharp
using System.Linq;

factory.FocusedDockableChanged += (_, e) =>
    Console.WriteLine($"Current document: {e.Dockable?.Title}");

var activeRoot = factory
    .Find(d => d is IRootDock root && root.IsActive)
    .OfType<IRootDock>()
    .FirstOrDefault();

var current = activeRoot?.FocusedDockable as IDocument;
```

```csharp
// Close whichever dockable currently has focus
if (activeRoot?.FocusedDockable is { } dockable)
{
    factory.CloseDockable(dockable);
}
```

The focused dockable comes from the active root dock. When no document is focused,
`FocusedDockable` will be `null`.
