// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class DocumentDock : DockBase, IDocumentDock
{
    private DocumentTabLayout _tabsLayout = DocumentTabLayout.Top;

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = ReactiveCommand.Create(CreateNewDocument);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanCreateDocument { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    public Func<IDockable>? DocumentFactory { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool EnableWindowDrag { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentTabLayout TabsLayout
    {
        get => _tabsLayout;
        set => this.RaiseAndSetIfChanged(ref _tabsLayout, value);
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

    /// <summary>
    /// Starts flashing the specified dockable tab.
    /// </summary>
    /// <param name="dockable">The dockable to flash.</param>
    public void FlashDockable(IDockable dockable)
    {
        Factory?.FlashDockable(dockable);
    }

    /// <summary>
    /// Stops flashing the specified dockable tab.
    /// </summary>
    /// <param name="dockable">The dockable to stop flashing.</param>
    public void StopFlashingDockable(IDockable dockable)
    {
        Factory?.StopFlashingDockable(dockable);
    }
}
