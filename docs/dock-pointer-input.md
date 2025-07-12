# Pointer Input

Dock uses Avalonia's pointer events which unify all pointer devices. This means mouse clicks, touch gestures and pen presses all invoke the same drag logic. No additional configuration is required.

Any pointer can drag tabs, resize docks or reposition floating windows. Features such as `EnableWindowDrag` or global docking work the same whether the user is using a mouse, touch screen or stylus.

Use the standard Avalonia `PointerPressed`, `PointerMoved` and `PointerReleased` events when extending Dock. These events automatically report the pointer type if you need to provide device-specific feedback.

For more details on floating windows see [Floating Windows](dock-windows.md) and [Enable Window Drag](dock-window-drag.md).
