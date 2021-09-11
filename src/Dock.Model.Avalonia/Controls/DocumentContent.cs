using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Document content.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DocumentContent : Document, IDocumentContent
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<DocumentContent, object>(nameof(Content));

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
