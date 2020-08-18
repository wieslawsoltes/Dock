using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Document.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Document : DockableBase, IDocument
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<Document, object>(nameof(Content));

        /// <summary>
        /// Gets or sets the content to display.
        /// </summary>
        [Content]
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

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
