
using System.Text.Json.Serialization;

namespace Dock.Model.Core
{
    /// <summary>
    /// Host window state contract.
    /// </summary>
    public interface IHostWindowState
    {
        /// <summary>
        /// Gets or sets dock manager.
        /// </summary>
        [JsonIgnore]
        IDockManager DockManager { get; set; }
    }
}
