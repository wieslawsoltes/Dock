using Dock.Model.Core;

namespace Dock.Model.Strategies;

internal class NoneDockOperationStrategy : IDockOperationStrategy
{
    public bool Execute(IDockable source, IDock sourceOwner, IDock targetDock, bool execute)
    {
        return false;
    }
}
