// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Core;

namespace Dock.Model.Inpc.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    private bool _canCreateDocument;
    private bool _enableWindowDrag;
    private DocumentLayoutMode _layoutMode = DocumentLayoutMode.Tabbed;
    private DocumentCloseButtonShowMode _closeButtonShowMode = DocumentCloseButtonShowMode.Always;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = new SimpleCommand(CreateNewDocument);
        CascadeDocuments = new SimpleCommand(CascadeDocumentsExecute);
        TileDocumentsHorizontal = new SimpleCommand(TileDocumentsHorizontalExecute);
        TileDocumentsVertical = new SimpleCommand(TileDocumentsVerticalExecute);
        RestoreDocuments = new SimpleCommand(RestoreDocumentsExecute);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCreateDocument
    {
        get => _canCreateDocument;
        set => SetProperty(ref _canCreateDocument, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CascadeDocuments { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? TileDocumentsHorizontal { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? TileDocumentsVertical { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? RestoreDocuments { get; set; }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    public Func<IDockable>? DocumentFactory { get; set; }

    private DocumentTabLayout _tabsLayout = DocumentTabLayout.Top;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableWindowDrag
    {
        get => _enableWindowDrag;
        set => SetProperty(ref _enableWindowDrag, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentLayoutMode LayoutMode
    {
        get => _layoutMode;
        set => SetProperty(ref _layoutMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentTabLayout TabsLayout
    {
        get => _tabsLayout;
        set => SetProperty(ref _tabsLayout, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentCloseButtonShowMode CloseButtonShowMode
    {
        get => _closeButtonShowMode;
        set => SetProperty(ref _closeButtonShowMode, value);
    }

    private void CreateNewDocument()
    {
        if (DocumentFactory is { } factory)
        {
            var document = factory();
            AddDocument(document);
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
}
