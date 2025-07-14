using Dock.Model.Core;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class PopupHostWindowState : DockManagerState, IHostWindowState
{
    private readonly PopupHostWindow _popup;

    public PopupHostWindowState(IDockManager dockManager, PopupHostWindow popup)
        : base(dockManager)
    {
        _popup = popup;
    }
}
