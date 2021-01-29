using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock control contract.
    /// </summary>
    public interface IDockControl
    {
        /// <summary>
        /// Gets dock controls.
        /// </summary>
        IList<IDockControl> DockControls { get; }

        /// <summary>
        /// Gets dock manager.
        /// </summary>
        IDockManager DockManager { get; }

        /// <summary>
        /// Gets dock control state.
        /// </summary>
        IDockControlState DockControlState { get; }

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        IDock? Layout { get; set; }

        /// <summary>
        /// Gets or sets the dock factory.
        /// </summary>
        IFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to initialize layout.
        /// </summary>
        bool InitializeLayout { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to initialize factory.
        /// </summary>
        bool InitializeFactory { get; set; }
    }
}
