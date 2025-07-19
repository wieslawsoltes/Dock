// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
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

    /// <summary>
    /// Defines the <see cref="EnableWindowDrag"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableWindowDragProperty =
        AvaloniaProperty.Register<DocumentDock, bool>(nameof(EnableWindowDrag));

    /// <summary>
    /// Defines the <see cref="TabsLayout"/> property.
    /// </summary>
    public static readonly StyledProperty<DocumentTabLayout> TabsLayoutProperty =
        AvaloniaProperty.Register<DocumentDock, DocumentTabLayout>(nameof(TabsLayout), DocumentTabLayout.Top);

    private bool _canCreateDocument;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = new Command(CreateNewDocument);
    }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public Func<IDockable>? DocumentFactory { get; set; }

    /// <summary>
    /// Gets or sets asynchronous factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public Func<Task<IDockable>>? DocumentFactoryAsync { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanCreateDocument")]
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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("TabsLayout")]
    public DocumentTabLayout TabsLayout
    {
        get => GetValue(TabsLayoutProperty);
        set => SetValue(TabsLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets document template.
    /// </summary>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("EnableWindowDrag")]
    public bool EnableWindowDrag
    {
        get => GetValue(EnableWindowDragProperty);
        set => SetValue(EnableWindowDragProperty, value);
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

    private async void CreateNewDocument()
    {
        if (DocumentFactoryAsync is { } asyncFactory)
        {
            var document = await asyncFactory();
            await AddDocumentAsync(document);
        }
        else if (DocumentFactory is { } factory)
        {
            var document = factory();
            AddDocument(document);
        }
        else
        {
            CreateDocumentFromTemplate();
        }
    }

    /// <summary>
    /// Adds the specified document to this dock and makes it active and focused.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public virtual void AddDocument(IDockable document)
    {
        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }

    /// <summary>
    /// Adds the specified document to this dock asynchronously and makes it active and focused.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public virtual async Task AddDocumentAsync(IDockable document)
    {
        if (Factory is not null)
        {
            await Factory.AddDocumentAsync(this, document);
        }
    }

    /// <summary>
    /// Adds the specified tool to this dock and makes it active and focused.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    public virtual void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }
}
