// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    private bool _canCreateDocument = true;
    private ICommand? _createDocument;
    private bool _enableWindowDrag = true;
    private DocumentTabLayout _tabsLayout = DocumentTabLayout.Top;
    private DocumentPresentation _presentation = DocumentPresentation.Tabs;

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableWindowDrag
    {
        get => _enableWindowDrag;
        set => Set(ref _enableWindowDrag, value);
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
    public DocumentPresentation Presentation
    {
        get => _presentation;
        set => Set(ref _presentation, value);
    }

    /// <inheritdoc/>
    public void CascadeDocuments() => Factory?.CascadeDocuments(this);

    /// <inheritdoc/>
    public void TileDocumentsHorizontally() => Factory?.TileDocumentsHorizontally(this);

    /// <inheritdoc/>
    public void TileDocumentsVertically() => Factory?.TileDocumentsVertically(this);

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