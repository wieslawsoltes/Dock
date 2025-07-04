namespace Dock.Avalonia.Contract;

using global::Avalonia;
using global::Avalonia.Controls;
using Dock.Avalonia.Controls;

public interface IDragOffsetCalculator
{
    PixelPoint CalculateOffset(Control dragControl, DockControl dockControl, Point pointerPosition);
}
