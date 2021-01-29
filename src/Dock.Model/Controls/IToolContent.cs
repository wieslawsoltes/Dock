
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
        object Content { get; set; }
    }
}
