# Dock Geometry Types

Dock uses lightweight geometry structs to describe positions and bounds in the model layer. These types are framework-agnostic and appear in docking services, MDI layouts, and serialization.

## DockPoint

`DockPoint` stores an X/Y coordinate pair:

- `X`
- `Y`

It is used by `DockManager` to track pointer positions during docking:

```csharp
var dockManager = new DockManager(new DockService())
{
    Position = new DockPoint(120, 80),
    ScreenPosition = new DockPoint(640, 360)
};
```

`Position` is the pointer location relative to the active dock control, while `ScreenPosition` is the same pointer position in screen coordinates. These values are used when docking into a new window.

## DockRect

`DockRect` stores a rectangle:

- `X`
- `Y`
- `Width`
- `Height`

The model layer uses it to record MDI window bounds. For example, `IMdiDocument.MdiBounds` is a `DockRect` that is updated by layout helpers and by `MdiDocumentWindow` interactions:

```csharp
document.MdiBounds = new DockRect(16, 16, 640, 480);
```

`DockRect` is marked with `DataContract` attributes so it is persisted with serialized layouts.

## Related guides

- [DockManager guide](dock-manager-guide.md)
- [MDI document layout](dock-mdi.md)
- [MDI layout helpers](dock-mdi-layout-helpers.md)
