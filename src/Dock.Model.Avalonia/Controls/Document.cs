using System;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Document.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Document : DockableBase, IDocument, IDocumentContent, ITemplate<Control>, IRecyclingDataTemplate
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
        [TemplateContent]
        [IgnoreDataMember]
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public Type? DataType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Control Build()
        {
            return (Control)Load(Content!).Control;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object? ITemplate.Build() => Build();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Match(object data)
        {
            if (DataType == null)
            {
                return true;
            }
            else
            {
                return DataType.IsInstanceOfType(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IControl Build(object data) => Build(data, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="existing"></param>
        /// <returns></returns>
        public IControl Build(object data, IControl? existing)
        {
            return existing ?? TemplateContent.Load(Content)?.Control!;
        }

        private static ControlTemplateResult Load(object templateContent)
        {
            if (templateContent is Func<IServiceProvider, object> direct)
            {
                return (ControlTemplateResult)direct(null!);
            }
            throw new ArgumentException(nameof(templateContent));
        }
    }
}
