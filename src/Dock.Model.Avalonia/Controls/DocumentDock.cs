// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock, IDocumentDockContent, IItemsSourceDock
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

    /// <summary>
    /// Defines the <see cref="LayoutMode"/> property.
    /// </summary>
    public static readonly StyledProperty<DocumentLayoutMode> LayoutModeProperty =
        AvaloniaProperty.Register<DocumentDock, DocumentLayoutMode>(nameof(LayoutMode), DocumentLayoutMode.Tabbed);

    /// <summary>
    /// Defines the <see cref="CloseButtonShowMode"/> property.
    /// </summary>
    public static readonly StyledProperty<DocumentCloseButtonShowMode> CloseButtonShowModeProperty =
        AvaloniaProperty.Register<DocumentDock, DocumentCloseButtonShowMode>(nameof(CloseButtonShowMode), DocumentCloseButtonShowMode.Always);

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DocumentDock, IEnumerable?>(nameof(ItemsSource));

    private bool _canCreateDocument;
    private readonly HashSet<IDockable> _generatedDocuments = new();
    private IDisposable? _itemsSourceSubscription;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = new Command(CreateNewDocument);
        CascadeDocuments = new Command(CascadeDocumentsExecute);
        TileDocumentsHorizontal = new Command(TileDocumentsHorizontalExecute);
        TileDocumentsVertical = new Command(TileDocumentsVerticalExecute);
        RestoreDocuments = new Command(RestoreDocumentsExecute);
        
        // Subscribe to ItemsSource property changes
        _itemsSourceSubscription = this.GetObservable(ItemsSourceProperty).Subscribe(new AnonymousObserver<IEnumerable?>(OnItemsSourceChanged));
    }

    /// <summary>
    /// Disposes the resources used by this DocumentDock.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from ItemsSource changes
            _itemsSourceSubscription?.Dispose();
            _itemsSourceSubscription = null;

            // Unsubscribe from collection changes
            if (_currentCollectionChanged != null)
            {
                _currentCollectionChanged.CollectionChanged -= OnCollectionChanged;
                _currentCollectionChanged = null;
            }

            // Clear generated documents
            ClearGeneratedDocuments();
        }
    }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public Func<IDockable>? DocumentFactory { get; set; }

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

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand? CascadeDocuments { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand? TileDocumentsHorizontal { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand? TileDocumentsVertical { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand? RestoreDocuments { get; set; }

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CloseButtonShowMode")]
    public DocumentCloseButtonShowMode CloseButtonShowMode
    {
        get => GetValue(CloseButtonShowModeProperty);
        set => SetValue(CloseButtonShowModeProperty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("LayoutMode")]
    public DocumentLayoutMode LayoutMode
    {
        get => GetValue(LayoutModeProperty);
        set => SetValue(LayoutModeProperty, value);
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
    /// Gets or sets the collection of items used to generate documents.
    /// When set, documents will be automatically created for each item using the DocumentTemplate.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
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

    private void CreateNewDocument()
    {
        if (DocumentFactory is { } factory)
        {
            var document = factory();
            AddDocument(document);
        }
        else
        {
            CreateDocumentFromTemplate();
        }
    }

    private void CascadeDocumentsExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.CascadeDocuments(this);
    }

    private void TileDocumentsHorizontalExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.TileDocumentsHorizontal(this);
    }

    private void TileDocumentsVerticalExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.TileDocumentsVertical(this);
    }

    private void RestoreDocumentsExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.RestoreDocuments(this);
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
    /// Adds the specified tool to this dock and makes it active and focused.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    public virtual void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }

    private INotifyCollectionChanged? _currentCollectionChanged;

    private void OnItemsSourceChanged(IEnumerable? newItemsSource)
    {
        System.Diagnostics.Debug.WriteLine($"DocumentDock.OnItemsSourceChanged called with: {newItemsSource?.GetType().Name}, Count: {newItemsSource?.Cast<object>().Count() ?? 0}");
        
        // Unsubscribe from old collection
        if (_currentCollectionChanged != null)
        {
            _currentCollectionChanged.CollectionChanged -= OnCollectionChanged;
            _currentCollectionChanged = null;
        }

        // Remove all documents that were generated from ItemsSource
        ClearGeneratedDocuments();

        // Subscribe to new collection if it supports change notifications
        if (newItemsSource is INotifyCollectionChanged notifyCollection)
        {
            _currentCollectionChanged = notifyCollection;
            _currentCollectionChanged.CollectionChanged += OnCollectionChanged;
            System.Diagnostics.Debug.WriteLine("Subscribed to collection change notifications");
        }

        // Generate documents for new collection
        if (newItemsSource != null)
        {
            int count = 0;
            foreach (var item in newItemsSource)
            {
                System.Diagnostics.Debug.WriteLine($"Adding document for item {count}: {item}");
                AddDocumentFromItem(item);
                count++;
            }
            System.Diagnostics.Debug.WriteLine($"Added {count} documents to DocumentDock. VisibleDockables count: {VisibleDockables?.Count ?? 0}");
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddDocumentFromItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveDocumentFromItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveDocumentFromItem(item);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddDocumentFromItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                ClearGeneratedDocuments();
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        AddDocumentFromItem(item);
                    }
                }
                break;
        }
    }

    private void AddDocumentFromItem(object? item)
    {
        if (item == null)
            return;

        // If there's no DocumentTemplate at all, don't create documents
        if (DocumentTemplate == null)
            return;

        // Create a new Document using the DocumentTemplate
        var document = new Document
        {
            Id = Guid.NewGuid().ToString(),
            Title = GetDocumentTitle(item),
            Context = item, // Set the data context to the item
            CanClose = GetDocumentCanClose(item)
        };

        // Use the DocumentTemplate to create content by setting the Content to a function
        // that builds the template with the item as data context
        // Set up document content using DocumentTemplate if available
        if (DocumentTemplate is DocumentTemplate template && template.Content != null)
        {
            // Use the DocumentTemplate.Content directly - let the Document handle the template building
            document.Content = template.Content;
        }
        else
        {
            // Template exists but has no content, create fallback content
            document.Content = new Func<IServiceProvider, object>(_ => CreateFallbackContent(document, item));
        }

        // Add to our tracking collection
        _generatedDocuments.Add(document);

        // Use the proper AddDocument API if Factory is available, otherwise add manually
        if (Factory != null)
        {
            // Use the proper AddDocument API which handles adding, making active, and focused
            AddDocument(document);
        }
        else
        {
            // Fallback for unit tests or when no Factory is set
            if (VisibleDockables == null)
            {
                VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>();
            }

            VisibleDockables.Add(document);

            // Set as active if it's the first document
            if (VisibleDockables.Count == 1)
            {
                ActiveDockable = document;
            }
        }
    }

    private Control CreateFallbackContent(Document document, object? item)
    {
        var contentPanel = new StackPanel { Margin = new Thickness(10) };
        
        // First TextBlock: Document title
        var titleBlock = new TextBlock 
        { 
            Text = document.Title ?? "Document", 
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Background = Brushes.LightBlue,
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentPanel.Children.Add(titleBlock);
        
        // Second TextBlock: Item's ToString() representation
        var contentBlock = new TextBlock 
        { 
            Text = item?.ToString() ?? "No content",
            Background = Brushes.LightGray,
            Padding = new Thickness(5),
            TextWrapping = TextWrapping.Wrap
        };
        contentPanel.Children.Add(contentBlock);
        
        // Set DataContext to the item (as expected by tests)
        contentPanel.DataContext = item;
        return contentPanel;
    }

    private void RemoveDocumentFromItem(object? item)
    {
        if (item == null)
            return;

        // Find the document that corresponds to this item
        var documentToRemove = _generatedDocuments
            .OfType<Document>()
            .FirstOrDefault(d => ReferenceEquals(d.Context, item));

        if (documentToRemove != null)
        {
            _generatedDocuments.Remove(documentToRemove);
            VisibleDockables?.Remove(documentToRemove);
        }
    }

    private void ClearGeneratedDocuments()
    {
        if (VisibleDockables != null)
        {
            // Remove all generated documents from VisibleDockables
            var documentsToRemove = _generatedDocuments.ToList();
            foreach (var document in documentsToRemove)
            {
                VisibleDockables.Remove(document);
            }
        }

        _generatedDocuments.Clear();
    }

    private string GetDocumentTitle(object item)
    {
        // Try to get title from common properties
        var type = item.GetType();
        
        // Check for Title property
        var titleProperty = type.GetProperty("Title");
        if (titleProperty?.GetValue(item) is string title)
            return title;

        // Check for Name property
        var nameProperty = type.GetProperty("Name");
        if (nameProperty?.GetValue(item) is string name)
            return name;

        // Check for DisplayName property
        var displayNameProperty = type.GetProperty("DisplayName");
        if (displayNameProperty?.GetValue(item) is string displayName)
            return displayName;

        // Fallback to ToString or type name
        return item.ToString() ?? type.Name;
    }

    private bool GetDocumentCanClose(object item)
    {
        // Try to get CanClose from the item
        var type = item.GetType();
        var canCloseProperty = type.GetProperty("CanClose");
        if (canCloseProperty?.GetValue(item) is bool canClose)
            return canClose;

        // Default to true
        return true;
    }

    /// <summary>
    /// Removes an item from the ItemsSource collection if it supports removal.
    /// This method is called by the factory when a document generated from ItemsSource is closed.
    /// Only supports collections that implement IList.
    /// </summary>
    /// <param name="item">The item to remove from the ItemsSource collection.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    public virtual bool RemoveItemFromSource(object? item)
    {
        if (item == null || ItemsSource == null)
            return false;

        // Only support IList<T> or IList collections
        if (ItemsSource is System.Collections.IList list)
        {
            if (list.Contains(item))
            {
                list.Remove(item);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a document was generated from ItemsSource.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document was generated from ItemsSource, false otherwise.</returns>
    public virtual bool IsDocumentFromItemsSource(IDockable document)
    {
        return _generatedDocuments.Contains(document);
    }
}
