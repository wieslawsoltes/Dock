using Dock.Model.Core;

namespace Dock.Model.Strategies;

internal class FillDockOperationStrategy : IDockOperationStrategy
{
    private readonly DockService _dockService;

    public FillDockOperationStrategy(DockService dockService)
    {
        _dockService = dockService;
    }

    public bool Execute(IDockable source, IDock sourceOwner, IDock targetDock, bool execute)
    {
        return _dockService.MoveDockable(source, sourceOwner, targetDock, execute);
    }
}
