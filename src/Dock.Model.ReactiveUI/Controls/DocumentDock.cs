using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls
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
