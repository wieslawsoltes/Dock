using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Document dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DocumentDock : DockBase, IDocumentDock
    {
        private bool _canCreateDocument;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanCreateDocument
        {
            get => _canCreateDocument;
            set => this.RaiseAndSetIfChanged(ref _canCreateDocument, value);
        }

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
