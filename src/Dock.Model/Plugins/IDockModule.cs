using Dock.Model.Core;

namespace Dock.Model.Plugins;

/// <summary>
/// Defines a module that registers dockables with a factory.
/// </summary>
public interface IDockModule
{
    /// <summary>
    /// Registers dockables using the provided factory instance.
    /// </summary>
    /// <param name="factory">The factory to register dockables with.</param>
    void Register(IFactory factory);
}

