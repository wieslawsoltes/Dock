﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    private bool _canCreateDocument;
    private bool _enableWindowDrag;
    private bool _removeTabOnDragOut;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
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
    public DocumentTabLayout TabsLayout
    {
        get => _tabsLayout;
        set => SetProperty(ref _tabsLayout, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool RemoveTabOnDragOut
    {
        get => _removeTabOnDragOut;
        set => SetProperty(ref _removeTabOnDragOut, value);
    }

    private void CreateNewDocument()
    {
        if (DocumentFactory is { } factory)
        {
            var document = factory();
            AddDocument(document);
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
