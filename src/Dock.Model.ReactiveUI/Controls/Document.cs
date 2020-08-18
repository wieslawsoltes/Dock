using System.Runtime.Serialization;

namespace Dock.Model.Controls
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

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return this;
        }
    }
}
