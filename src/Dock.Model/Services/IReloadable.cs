using System.Threading.Tasks;

namespace Dock.Model.Services;

/// <summary>
/// Provides a reload contract for dockables or routed views.
/// </summary>
public interface IReloadable
{
    /// <summary>
    /// Reloads the current view state.
    /// </summary>
    Task ReloadAsync();
}
