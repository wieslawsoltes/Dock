using System;

namespace Dock.Model.Core
{
    /// <summary>
    /// Dock control contract.
    /// </summary>
    public interface IDockControl
    {
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
        /// Gets or sets the flag indicating whether to initialize layout.
        /// </summary>
        bool InitializeLayout { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to initialize factory.
        /// </summary>
        bool InitializeFactory { get; set; }

        /// <summary>
        /// Gets or sets the factory type.
        /// </summary>
        Type? FactoryType { get; set; }
    }
}
