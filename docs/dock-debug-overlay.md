# Debug overlay

The debug overlay highlights docking-related controls in the visual tree.
When enabled each `DockControl` gets an adorner that draws:

- Dock targets in **red**
- Drag areas in **green**
- Drop areas in **blue**

Hovering over an area fills it with a translucent color and shows the hovered
control's data context in the bottom-right corner.

Enable the overlay in your application just like Avalonia's dev tools:

```csharp
#if DEBUG
this.AttachDockDebugOverlay();
#endif
```

Call the extension method on your `App` or on a specific `Window`/`TopLevel` to
activate the overlay.
