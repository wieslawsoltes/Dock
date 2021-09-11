using System.Runtime.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Document.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class Document : DockableBase, IDocument
    {
        /// <summary>
        /// Initializes new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document()
        {
            Id = nameof(IDocument);
            Title = nameof(IDocument);
        }
    }
}
