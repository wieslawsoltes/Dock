using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool content.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToolContent : Tool, IToolContent
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<ToolContent, object>(nameof(Content));

        /// <summary>
        /// Gets or sets the content to display.
        /// </summary>
        [Content]
        [IgnoreDataMember]
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
    }
}
