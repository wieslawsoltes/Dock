namespace Dock.Avalonia.Internal;

using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

public interface IDragOffsetCalculator
{
    PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition);
}
