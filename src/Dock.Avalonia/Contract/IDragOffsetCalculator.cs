using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Contract;

public interface IDragOffsetCalculator
{
    PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition);
}
