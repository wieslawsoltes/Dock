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
using Avalonia.Reactive;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document dock.
/// </summary>
public class DocumentDock : DockBase, IDocumentDock, IDocumentDockContent, IItemsSourceDock, IDocumentDockFactory
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
    /// Defines the <see cref="EmptyContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyContentProperty =
        AvaloniaProperty.Register<DocumentDock, object?>(nameof(EmptyContent), "No documents open");

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DocumentDock, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="ItemContainerGenerator"/> property.
    /// </summary>
    public static readonly StyledProperty<IDockItemContainerGenerator?> ItemContainerGeneratorProperty =
        AvaloniaProperty.Register<DocumentDock, IDockItemContainerGenerator?>(nameof(ItemContainerGenerator));

    private bool _canCreateDocument;
    private readonly HashSet<IDockable> _generatedDocuments = new();
    private readonly Dictionary<IDockable, IDockItemContainerGenerator> _generatedDocumentGenerators = new();
    private IDisposable? _itemsSourceSubscription;
    private IDisposable? _itemContainerGeneratorSubscription;

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
        _itemContainerGeneratorSubscription = this.GetObservable(ItemContainerGeneratorProperty)
            .Subscribe(new AnonymousObserver<IDockItemContainerGenerator?>(_ => OnItemContainerGeneratorChanged()));
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
            _itemContainerGeneratorSubscription?.Dispose();
            _itemContainerGeneratorSubscription = null;

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
    [IgnoreDataMember]
    [JsonIgnore]
    public object? EmptyContent
    {
        get => GetValue(EmptyContentProperty);
        set => SetValue(EmptyContentProperty, value);
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
    /// Gets or sets the generator used to create and prepare containers for ItemsSource items.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDockItemContainerGenerator? ItemContainerGenerator
    {
        get => GetValue(ItemContainerGeneratorProperty);
        set => SetValue(ItemContainerGeneratorProperty, value);
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

    private IDockItemContainerGenerator ResolveItemContainerGenerator()
    {
        return ItemContainerGenerator ?? DockItemContainerGenerator.Default;
    }

    private void OnItemContainerGeneratorChanged()
    {
        if (ItemsSource is null)
        {
            return;
        }

        RegenerateGeneratedDocuments(ItemsSource);
    }

    private void RegenerateGeneratedDocuments(IEnumerable itemsSource)
    {
        ClearGeneratedDocuments();

        var index = 0;
        foreach (var item in itemsSource)
        {
            AddDocumentFromItem(item, index);
            index++;
        }
    }

    private void OnItemsSourceChanged(IEnumerable? newItemsSource)
    {
        if (_currentCollectionChanged != null)
        {
            _currentCollectionChanged.CollectionChanged -= OnCollectionChanged;
            _currentCollectionChanged = null;
        }

        if (newItemsSource is INotifyCollectionChanged notifyCollection)
        {
            _currentCollectionChanged = notifyCollection;
            _currentCollectionChanged.CollectionChanged += OnCollectionChanged;
        }

        if (newItemsSource is null)
        {
            ClearGeneratedDocuments();
            return;
        }

        RegenerateGeneratedDocuments(newItemsSource);
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    var addIndex = e.NewStartingIndex;
                    var offset = 0;
                    foreach (var item in e.NewItems)
                    {
                        AddDocumentFromItem(item, addIndex >= 0 ? addIndex + offset : -1);
                        offset++;
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
                    var replaceIndex = e.NewStartingIndex;
                    var offset = 0;
                    foreach (var item in e.NewItems)
                    {
                        AddDocumentFromItem(item, replaceIndex >= 0 ? replaceIndex + offset : -1);
                        offset++;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                if (ItemsSource != null)
                {
                    RegenerateGeneratedDocuments(ItemsSource);
                }
                else
                {
                    ClearGeneratedDocuments();
                }
                break;
        }
    }

    private void AddDocumentFromItem(object? item, int index)
    {
        if (item == null)
        {
            return;
        }

        var generator = ResolveItemContainerGenerator();
        var document = generator.CreateDocumentContainer(this, item, index);
        if (document is null)
        {
            return;
        }

        if (document is not IDocument)
        {
            generator.ClearDocumentContainer(this, document, item);
            return;
        }

        generator.PrepareDocumentContainer(this, document, item, index);

        _generatedDocuments.Add(document);
        _generatedDocumentGenerators[document] = generator;
        TrackItemsSourceDocument(document);

        if (Factory != null)
        {
            AddDocument(document);
            return;
        }

        if (VisibleDockables == null)
        {
            VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>();
        }

        VisibleDockables.Add(document);
        if (VisibleDockables.Count == 1)
        {
            ActiveDockable = document;
        }
    }

    private void RemoveDocumentFromItem(object? item)
    {
        if (item == null)
        {
            return;
        }

        var documentToRemove = FindGeneratedDocument(item);

        if (documentToRemove != null)
        {
            _generatedDocuments.Remove(documentToRemove);
            UntrackItemsSourceDocument(documentToRemove);
            ClearGeneratedDocumentContainer(documentToRemove, item);
            RemoveGeneratedDocumentFromVisibleDockables(documentToRemove);
        }
    }

    private void ClearGeneratedDocuments()
    {
        foreach (var document in _generatedDocuments.ToList())
        {
            UntrackItemsSourceDocument(document);
            ClearGeneratedDocumentContainer(document, document.Context);
            RemoveGeneratedDocumentFromVisibleDockables(document);
        }

        _generatedDocuments.Clear();
        _generatedDocumentGenerators.Clear();
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
        if (item == null)
        {
            return false;
        }

        // Only support IList<T> or IList collections
        if (ItemsSource is System.Collections.IList list)
        {
            if (list.Contains(item))
            {
                try
                {
                    list.Remove(item);

                    // Non-notify sources will not raise collection changed events.
                    if (_currentCollectionChanged is null)
                    {
                        UntrackGeneratedDocument(item);
                    }

                    return true;
                }
                catch (NotSupportedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        UntrackGeneratedDocument(item);
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

    private void UntrackGeneratedDocument(object item)
    {
        var generatedDocument = FindGeneratedDocument(item);

        if (generatedDocument != null)
        {
            _generatedDocuments.Remove(generatedDocument);
            UntrackItemsSourceDocument(generatedDocument);
            ClearGeneratedDocumentContainer(generatedDocument, item);
        }
    }

    private IDockable? FindGeneratedDocument(object item)
    {
        foreach (var generatedDocument in _generatedDocuments)
        {
            if (IsMatchingContext(generatedDocument.Context, item))
            {
                return generatedDocument;
            }
        }

        return null;
    }

    private static bool IsMatchingContext(object? context, object? item)
    {
        if (ReferenceEquals(context, item))
        {
            return true;
        }

        return Equals(context, item);
    }

    private void ClearGeneratedDocumentContainer(IDockable document, object? item)
    {
        if (_generatedDocumentGenerators.TryGetValue(document, out var generator))
        {
            _generatedDocumentGenerators.Remove(document);
            generator.ClearDocumentContainer(this, document, item);
            return;
        }

        ResolveItemContainerGenerator().ClearDocumentContainer(this, document, item);
    }

    private void RemoveGeneratedDocumentFromVisibleDockables(IDockable document)
    {
        if (document.Owner is IDock owner)
        {
            if (owner.VisibleDockables?.Remove(document) == true && ReferenceEquals(owner.ActiveDockable, document))
            {
                owner.ActiveDockable = owner.VisibleDockables.FirstOrDefault();
            }
            return;
        }

        if (VisibleDockables?.Remove(document) == true && ReferenceEquals(ActiveDockable, document))
        {
            ActiveDockable = VisibleDockables.FirstOrDefault();
        }
    }

    private void TrackItemsSourceDocument(IDockable document)
    {
        if (Factory is global::Dock.Model.FactoryBase factoryBase)
        {
            factoryBase.TrackItemsSourceDockable(document, this);
        }
    }

    private void UntrackItemsSourceDocument(IDockable document)
    {
        if (Factory is global::Dock.Model.FactoryBase factoryBase)
        {
            factoryBase.UntrackItemsSourceDockable(document);
        }
    }
}
