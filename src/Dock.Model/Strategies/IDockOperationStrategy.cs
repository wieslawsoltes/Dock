using Dock.Model.Core;

namespace Dock.Model.Strategies;

internal interface IDockOperationStrategy
{
    bool Execute(IDockable source, IDock sourceOwner, IDock targetDock, bool execute);
}
