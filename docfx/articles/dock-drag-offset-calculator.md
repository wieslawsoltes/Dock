# Drag Offset Calculator

`IDragOffsetCalculator` defines how the drag preview window is positioned relative to the pointer. Dock uses the interface during drag operations to keep the preview aligned with the item being moved.

## Default behavior

`DockControl` creates a `DefaultDragOffsetCalculator` when no custom instance is provided. The default implementation keeps the preview locked to the tab being dragged so the window appears in the same place when released.

## Providing a custom calculator

Assign your own implementation to the `DragOffsetCalculator` property on `DockControl` to change the offset logic:

```csharp
public class CenteredOffsetCalculator : IDragOffsetCalculator
{
    public PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition)
    {
        var screenPoint = dockControl.PointToScreen(pointerPosition);
        var bounds = dragControl.Bounds;
        return new PixelPoint(-(int)(bounds.Width / 2), -(int)(bounds.Height / 2));
    }
}

var dockControl = new DockControl
{
    DragOffsetCalculator = new CenteredOffsetCalculator()
};
```

This example centres the preview on the pointer regardless of what was dragged. The method can inspect the `dragControl` and `dockControl` to calculate any position.

## When to customize

- Align the preview to a different part of the dragged control.
- Add margins so the preview does not obscure important UI elements.
- Support complex hit testing when multiple controls participate in the drag.

Custom calculators give full control over where the preview window appears, allowing smoother drag and drop experiences.

For other customization options see the [DockManager guide](dock-manager-guide.md).
