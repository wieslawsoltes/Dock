namespace Dock.Avalonia.Internal;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Contract;

public class DefaultDragOffsetCalculator : IDragOffsetCalculator
{
    public PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition)
    {
        var screenPoint = dockControl.PointToScreen(pointerPosition);

        if (dragControl.TemplatedParent is TabStripItem tabStripItem)
        {
            var corner = tabStripItem.PointToScreen(new Point());
            return corner - screenPoint;
        }

        return default;
    }
}
