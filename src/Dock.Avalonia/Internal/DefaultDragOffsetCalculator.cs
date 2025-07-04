using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Contract;

namespace Dock.Avalonia.Internal;

internal class DefaultDragOffsetCalculator : IDragOffsetCalculator
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
