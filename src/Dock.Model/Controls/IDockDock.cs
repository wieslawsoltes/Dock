
using System.Text.Json.Serialization;
using Dock.Model.Core;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Docking panel contract.
    /// </summary>
    public interface IDockDock : IDock
    {
        /// <summary>
        /// Gets or sets a value which indicates whether the last child of the fills the remaining space in the panel.
        /// </summary>
        [JsonInclude]
        bool LastChildFill { get; set; }
    }
}
