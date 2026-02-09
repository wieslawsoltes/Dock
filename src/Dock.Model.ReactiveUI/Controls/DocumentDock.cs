// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Document dock.
/// </summary>
public partial class DocumentDock : DockBase, IDocumentDock, IDocumentDockFactory
{
    private object? _emptyContent = "No documents open";
    private DocumentTabLayout _tabsLayout = DocumentTabLayout.Top;
    private DocumentLayoutMode _layoutMode = DocumentLayoutMode.Tabbed;
    private DocumentCloseButtonShowMode _closeButtonShowMode = DocumentCloseButtonShowMode.Always;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = ReactiveCommand.Create(CreateNewDocument);
        CascadeDocuments = ReactiveCommand.Create(CascadeDocumentsExecute);
        TileDocumentsHorizontal = ReactiveCommand.Create(TileDocumentsHorizontalExecute);
        TileDocumentsVertical = ReactiveCommand.Create(TileDocumentsVerticalExecute);
        RestoreDocuments = ReactiveCommand.Create(RestoreDocumentsExecute);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool CanCreateDocument { get; set; }

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool EnableWindowDrag { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentTabLayout TabsLayout
    {
        get => _tabsLayout;
        set => this.RaiseAndSetIfChanged(ref _tabsLayout, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentLayoutMode LayoutMode
    {
        get => _layoutMode;
        set => this.RaiseAndSetIfChanged(ref _layoutMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentCloseButtonShowMode CloseButtonShowMode
    {
        get => _closeButtonShowMode;
        set => this.RaiseAndSetIfChanged(ref _closeButtonShowMode, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public object? EmptyContent
    {
        get => _emptyContent;
        set => this.RaiseAndSetIfChanged(ref _emptyContent, value);
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
