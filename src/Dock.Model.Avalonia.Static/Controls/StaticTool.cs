using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Static tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public class StaticTool : Tool
    {
        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<Tool, object>(nameof(Content));

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
