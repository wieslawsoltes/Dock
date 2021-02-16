
using Dock.Model.Core;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Document content contract.
    /// </summary>
    public interface IDocumentContent : IDockable
    {
        /// <summary>
        /// Gets or sets document content.
        /// </summary>
        object Content { get; set; }
    }
}
