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
        /// Defines the <see cref="Template"/> property.
        /// </summary>
        public static readonly StyledProperty<object> TemplateProperty =
            AvaloniaProperty.Register<Tool, object>(nameof(Template));

        /// <summary>
        /// Initializes new instance of the <see cref="Tool"/> class.
        /// </summary>
        public Tool()
        {
            Id = nameof(ITool);
            Title = nameof(ITool);
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
