using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
[JsonSerializable(typeof(DocumentDock))]
public class DocumentDock : DockBase, IDocumentDock, IDocumentDockContent
{
    /// <summary>
    /// Defines the <see cref="CanCreateDocument"/> property.
    /// </summary>
    public static readonly DirectProperty<DocumentDock, bool> CanCreateDocumentProperty =
        AvaloniaProperty.RegisterDirect<DocumentDock, bool>(nameof(CanCreateDocument), o => o.CanCreateDocument, (o, v) => o.CanCreateDocument = v);

    /// <summary>
    /// Defines the <see cref="DocumentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDocumentTemplate?> DocumentTemplateProperty =
        AvaloniaProperty.Register<DocumentDock, IDocumentTemplate?>(nameof(DocumentTemplate));

    private bool _canCreateDocument;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    [JsonConstructor]
    public DocumentDock()
    {
        CreateDocument = new Command(() => CreateDocumentFromTemplate());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonInclude]
    public bool CanCreateDocument
    {
        get => _canCreateDocument;
        set => SetAndRaise(CanCreateDocumentProperty, ref _canCreateDocument, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand? CreateDocument { get; set; }

    /// <summary>
    /// Gets or sets document template.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDocumentTemplate? DocumentTemplate
    {
        get => GetValue(DocumentTemplateProperty);
        set => SetValue(DocumentTemplateProperty, value);
    }

    /// <summary>
    /// Creates new document from template.
    /// </summary>
    public virtual object? CreateDocumentFromTemplate()
    {
        if (DocumentTemplate is null || !CanCreateDocument)
        {
            return null;
        }

        var document = new Document
        {
            Title = $"Document{VisibleDockables?.Count ?? 0}",
            Content = DocumentTemplate.Content
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);

        return document;
    }
}
