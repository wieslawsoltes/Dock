using System.Collections.Generic;

namespace Dock.Model.Core
{
    /// <summary>
    /// Dockable control contract.
    /// </summary>
    public interface IDockableControl
    {
        /// <summary>
        /// Gets visible dockable controls.
        /// </summary>
        IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

        /// <summary>
        /// Gets pinned dockable controls.
        /// </summary>
        IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }
        
        /// <summary>
        /// Gets tab dockable controls.
        /// </summary>
        IDictionary<IDockable, IDockableControl> TabDockableControls { get; }
    }
}
