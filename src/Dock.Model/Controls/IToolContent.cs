
using System.Text.Json.Serialization;
using Dock.Model.Core;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool content contract.
    /// </summary>
    public interface IToolContent : IDockable
    {
        /// <summary>
        /// Gets or sets tool content.
        /// </summary>
        [JsonInclude]
        object Content { get; set; }
    }
}
