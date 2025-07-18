using Dock.Model.Core;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class OverlayHostWindowState : DockManagerState, IHostWindowState
{
    private readonly OverlayHostWindow _window;

    public OverlayHostWindowState(IDockManager dockManager, OverlayHostWindow window)
        : base(dockManager)
    {
        _window = window;
    }
}
