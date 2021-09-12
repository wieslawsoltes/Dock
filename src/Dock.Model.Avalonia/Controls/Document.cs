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
        /// Defines the <see cref="Template"/> property.
        /// </summary>
        public static readonly StyledProperty<object> TemplateProperty =
            AvaloniaProperty.Register<Document, object>(nameof(Template));

        /// <summary>
        /// Initializes new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document()
        {
            Id = nameof(IDocument);
            Title = nameof(IDocument);
        }

        /// <summary>
        /// Gets or sets the template to display.
        /// </summary>
        [Content]
        [IgnoreDataMember]
        public object Template
        {
            get => GetValue(TemplateProperty);
            set => SetValue(TemplateProperty, value);
        }
    }
}
