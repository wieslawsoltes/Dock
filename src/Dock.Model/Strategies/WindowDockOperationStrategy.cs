using System;
using Dock.Model.Core;

namespace Dock.Model.Strategies;

internal class WindowDockOperationStrategy : IDockOperationStrategy
{
    private readonly DockService _dockService;
    private readonly Func<DockPoint> _getScreenPosition;

    public WindowDockOperationStrategy(DockService dockService, Func<DockPoint> getScreenPosition)
    {
        _dockService = dockService;
        _getScreenPosition = getScreenPosition;
    }

    public bool Execute(IDockable source, IDock sourceOwner, IDock targetDock, bool execute)
    {
        return _dockService.DockDockableIntoWindow(source, targetDock, _getScreenPosition(), execute);
    }
}
