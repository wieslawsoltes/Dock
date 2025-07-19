# Drag Offset Sample

`DragOffsetSample` shows how to reposition the drag preview window by supplying a custom `IDragOffsetCalculator`.

```csharp
public class CenteredDragOffsetCalculator : IDragOffsetCalculator
{
    public PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition)
    {
        var bounds = dragControl.Bounds;
        return new PixelPoint(-(int)(bounds.Width / 2), -(int)(bounds.Height / 2));
    }
}
```

Assign the calculator to `DragOffsetCalculator` on `DockControl` before displaying it:

```csharp
var dockControl = new DockControl
{
    DragOffsetCalculator = new CenteredDragOffsetCalculator()
};
```

The preview window now stays centred on the pointer during the drag instead of sticking to the original tab position.

