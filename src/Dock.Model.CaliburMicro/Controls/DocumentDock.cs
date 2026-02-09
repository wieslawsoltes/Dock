// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Document dock.
/// </summary>
public class DocumentDock : DockBase, IDocumentDock, IDocumentDockFactory
{
    private object? _emptyContent = "No documents open";
    private bool _canCreateDocument = true;
    private ICommand? _createDocument;
    private bool _enableWindowDrag = true;
    private DocumentLayoutMode _layoutMode = DocumentLayoutMode.Tabbed;
    private DocumentTabLayout _tabsLayout = DocumentTabLayout.Top;
    private DocumentCloseButtonShowMode _closeButtonShowMode = DocumentCloseButtonShowMode.Always;
    private ICommand? _cascadeDocuments;
    private ICommand? _tileDocumentsHorizontal;
    private ICommand? _tileDocumentsVertical;
    private ICommand? _restoreDocuments;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCreateDocument
    {
        get => _canCreateDocument;
        set => Set(ref _canCreateDocument, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public ICommand? CreateDocument
    {
        get => _createDocument;
        set => Set(ref _createDocument, value);
    }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    public Func<IDockable>? DocumentFactory { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableWindowDrag
    {
        get => _enableWindowDrag;
        set => Set(ref _enableWindowDrag, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentLayoutMode LayoutMode
    {
        get => _layoutMode;
        set => Set(ref _layoutMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentTabLayout TabsLayout
    {
        get => _tabsLayout;
        set => Set(ref _tabsLayout, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentCloseButtonShowMode CloseButtonShowMode
    {
        get => _closeButtonShowMode;
        set => Set(ref _closeButtonShowMode, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public object? EmptyContent
    {
        get => _emptyContent;
        set => Set(ref _emptyContent, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public ICommand? CascadeDocuments
    {
        get => _cascadeDocuments;
        set => Set(ref _cascadeDocuments, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public ICommand? TileDocumentsHorizontal
    {
        get => _tileDocumentsHorizontal;
        set => Set(ref _tileDocumentsHorizontal, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public ICommand? TileDocumentsVertical
    {
        get => _tileDocumentsVertical;
        set => Set(ref _tileDocumentsVertical, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public ICommand? RestoreDocuments
    {
        get => _restoreDocuments;
        set => Set(ref _restoreDocuments, value);
    }

    /// <inheritdoc/>
    public void AddDocument(IDockable document)
    {
        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }

    /// <inheritdoc/>
    public void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }
}
