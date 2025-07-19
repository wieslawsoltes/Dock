using Dock.Model.Core;

namespace Dock.Model.Strategies;

internal class LeftDockOperationStrategy : IDockOperationStrategy
{
    private readonly DockService _dockService;

    public LeftDockOperationStrategy(DockService dockService)
    {
        _dockService = dockService;
    }

    public bool Execute(IDockable source, IDock sourceOwner, IDock targetDock, bool execute)
    {
        return _dockService.SplitDockable(source, sourceOwner, targetDock, DockOperation.Left, execute);
    }
}
