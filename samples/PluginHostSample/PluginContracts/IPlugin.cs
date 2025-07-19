using Dock.Model.Core;

namespace PluginContracts;

public interface IPlugin
{
    IDockable CreateDockable();
}

