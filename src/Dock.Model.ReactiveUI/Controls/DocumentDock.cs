using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
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

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand? CreateDocument { get; set; }
    }
}
