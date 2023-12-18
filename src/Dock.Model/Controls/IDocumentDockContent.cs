using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Document dock content contract.
/// </summary>
public interface IDocumentDockContent : IDock
{
    /// <summary>
    /// Gets or sets document template.
    /// </summary>
    IDocumentTemplate? DocumentTemplate { get; set; }

    /// <summary>
    /// Create new document from template.
    /// </summary>
    /// <returns>The new document instance.</returns>
    object? CreateDocumentFromTemplate();
}
