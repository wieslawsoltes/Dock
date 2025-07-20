# Finding the Active Document

Only one document or tool can have focus at a time, even when multiple
`IDocumentDock` instances or floating windows are present.  The
`IRootDock` that is currently active exposes its focused dockable via the
`FocusedDockable` property.  You can subscribe to the
`IFactory.FocusedDockableChanged` event or query the property directly.

```csharp
factory.FocusedDockableChanged += (_, e) =>
    Console.WriteLine($"Current document: {e.Dockable?.Title}");

var current = factory.GetCurrentDocument();
```

The helper `GetCurrentDocument` extension returns the focused document
for the active root dock or `null` if no document is selected.

```csharp
// Close whichever dockable currently has focus
factory.CloseFocusedDockable();
```

The `CloseFocusedDockable` helper can be used to close the active document or tool
without manually looking it up.
