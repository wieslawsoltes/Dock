using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Tool : DockableBase, ITool, IDocument
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
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Initializes new instance of the <see cref="Tool"/> class.
        /// </summary>
        public Tool()
        {
            Id = nameof(ITool);
            Title = nameof(ITool);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return this;
        }
    }
}
