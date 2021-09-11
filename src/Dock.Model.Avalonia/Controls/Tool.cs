using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Tool : DockableBase, ITool, IDocument, IToolContent
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<Document, object>(nameof(Content));

        /// <summary>
        /// Initializes new instance of the <see cref="Tool"/> class.
        /// </summary>
        public Tool()
        {
            Id = nameof(ITool);
            Title = nameof(ITool);
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
