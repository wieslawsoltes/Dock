using System.Runtime.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    /// <summary>
    /// Defines the <see cref="CanCreateDocument"/> property.
    /// </summary>
    public static readonly DirectProperty<DocumentDock, bool> CanCreateDocumentProperty =
        AvaloniaProperty.RegisterDirect<DocumentDock, bool>(nameof(CanCreateDocument), o => o.CanCreateDocument, (o, v) => o.CanCreateDocument = v);

    /// <summary>
    /// Defines the <see cref="DocumentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<DocumentTemplate?> DocumentTemplateProperty =
        AvaloniaProperty.Register<DocumentDock, DocumentTemplate?>(nameof(DocumentTemplate));

    private bool _canCreateDocument;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        Id = nameof(IDocumentDock);
        Title = nameof(IDocumentDock);
        CreateDocument = new Command(CreateDocumentFromTemplate);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCreateDocument
    {
        get => _canCreateDocument;
        set => SetAndRaise(CanCreateDocumentProperty, ref _canCreateDocument, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }

    /// <summary>
    /// Gets or sets document template.
    /// </summary>
    public DocumentTemplate? DocumentTemplate
    {
        get { return GetValue(DocumentTemplateProperty); }
        set { SetValue(DocumentTemplateProperty, value); }
    }

    /// <summary>
    /// Creates new document from template.
    /// </summary>
    protected virtual void CreateDocumentFromTemplate()
    {
        if (DocumentTemplate is null || !CanCreateDocument)
        {
            return;
        }

        var control = (DocumentTemplate as ITemplate)?.Build();
        if (control is IDockable dockable)
        {
            Factory?.AddDockable(this, dockable);
            Factory?.SetActiveDockable(dockable);
            Factory?.SetFocusedDockable(this, dockable);
        }
    }
}
