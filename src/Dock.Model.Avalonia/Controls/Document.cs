using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Document.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Document : DockableBase, IDocument, IDocumentContent
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<Document, object>(nameof(Content));

        /// <summary>
        /// Initializes new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document()
        {
            Id = nameof(IDocument);
            Title = nameof(IDocument);
        }

        /// <summary>
        /// Gets or sets the content to display.
        /// </summary>
        [Content]
        [IgnoreDataMember]
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
    }
}
