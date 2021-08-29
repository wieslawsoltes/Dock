using System.Text.Json.Serialization;

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
        [JsonIgnore]
        IDockManager DockManager { get; }

        /// <summary>
        /// Gets dock control state.
        /// </summary>
        [JsonIgnore]
        IDockControlState DockControlState { get; }

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        [JsonInclude]
        IDock? Layout { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to initialize layout.
        /// </summary>
        [JsonInclude]
        bool InitializeLayout { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to initialize factory.
        /// </summary>
        [JsonInclude]
        bool InitializeFactory { get; set; }
    }
}
