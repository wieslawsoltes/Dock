using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

internal interface ILayoutSystemMenu
{
    bool TryShow(Window window, PixelPoint position, bool canClose);
}
