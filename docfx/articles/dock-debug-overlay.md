# Debug overlay

The debug overlay highlights docking-related controls in the visual tree.
When enabled each `DockControl` gets an adorner that draws:

- Dock targets in **red**
- Drag areas in **green**
- Drop areas in **blue**

Hovering over an area fills it with a translucent color and shows the hovered
control's data context in the bottom-right corner.

Attach the overlay in your application just like Avalonia's dev tools. It can be
toggled at runtime using <kbd>F9</kbd> by default:

```csharp
#if DEBUG
// TopLevel/Window overload returns IDisposable so you can detach later.
var overlay = this.AttachDockDebugOverlay();
#endif
```

Call the extension method on your `App` or on a specific `Window`/`TopLevel` to
register the hot key. The `Application` overload attaches to existing windows
and does not return a disposable:

```csharp
#if DEBUG
Application.Current?.AttachDockDebugOverlay();
#endif
```

Optionally pass your own `KeyGesture` to change the shortcut.
