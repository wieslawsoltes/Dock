using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Document dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DocumentDock : DockBase, IDocumentDock
    {
        /// <summary>
        /// Initializes new instance of the <see cref="DocumentDock"/> class.
        /// </summary>
        public DocumentDock()
        {
            Id = nameof(IDocumentDock);
            Title = nameof(IDocumentDock);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.CloneDocumentDock(this);
        }
    }
}
