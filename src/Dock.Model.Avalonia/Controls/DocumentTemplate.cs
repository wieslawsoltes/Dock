using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document template.
/// </summary>
public class DocumentTemplate : ITemplate
{
    /// <summary>
    /// Gets or sets content.
    /// </summary>
    [Content]
    [TemplateContent(TemplateResultType = typeof(Document))]
    public object? Content { get; set; }

    /// <summary>
    /// Builds object from content.
    /// </summary>
    /// <returns>The built object from content.</returns>
    object ITemplate.Build() => TemplateContent.Load<Document>(Content).Result;
}
