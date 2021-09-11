using System.Runtime.Serialization;
using System.Windows.Input;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Document dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DocumentDock : DockBase, IDocumentDock
    {
        /// <summary>
        /// Defines the <see cref="CanCreateDocument"/> property.
        /// </summary>
        public static readonly DirectProperty<DocumentDock, bool> CanCreateDocumentProperty =
            AvaloniaProperty.RegisterDirect<DocumentDock, bool>(nameof(CanCreateDocument), o => o.CanCreateDocument, (o, v) => o.CanCreateDocument = v);

        private bool _canCreateDocument;

        /// <summary>
        /// Initializes new instance of the <see cref="DocumentDock"/> class.
        /// </summary>
        public DocumentDock()
        {
            Id = nameof(IDocumentDock);
            Title = nameof(IDocumentDock);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanCreateDocument
        {
            get => _canCreateDocument;
            set => SetAndRaise(CanCreateDocumentProperty, ref _canCreateDocument, value);
        }
        
        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand? CreateDocument { get; set; }
    }
}
