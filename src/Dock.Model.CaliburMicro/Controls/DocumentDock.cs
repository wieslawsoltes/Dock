// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Windows.Input;
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
    public void AddDocument(IDockable document)
    {
        // Implementation would be provided by the factory or dock manager
    }

    /// <inheritdoc/>
    public void AddTool(IDockable tool)
    {
        // Implementation would be provided by the factory or dock manager
    }
}