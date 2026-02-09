// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Document dock contract.
/// </summary>
[RequiresDataTemplate]
public interface IDocumentDock : IDock, ILocalTarget
{
    /// <summary>
    /// Gets or sets if document dock can create new documents.
    /// </summary>
    bool CanCreateDocument { get; set; }

    /// <summary>
    /// Gets or sets command to create new document.
    /// </summary>
    ICommand? CreateDocument { get; set; }

    /// <summary>
    /// Gets or sets if the window can be dragged by clicking on the tab strip.
    /// </summary>
    bool EnableWindowDrag { get; set;}

    /// <summary>
    /// Gets or sets document layout mode.
    /// </summary>
    DocumentLayoutMode LayoutMode { get; set; }

    /// <summary>
    /// Gets or sets document tabs layout.
    /// </summary>
    DocumentTabLayout TabsLayout { get; set; }

    /// <summary>
    /// Gets or sets when document close buttons are displayed.
    /// </summary>
    DocumentCloseButtonShowMode CloseButtonShowMode { get; set; }

    /// <summary>
    /// Gets or sets placeholder content shown when the document host has no visible dockables.
    /// </summary>
    object? EmptyContent { get; set; }

    /// <summary>
    /// Gets or sets command to cascade MDI documents.
    /// </summary>
    ICommand? CascadeDocuments { get; set; }

    /// <summary>
    /// Gets or sets command to tile MDI documents horizontally.
    /// </summary>
    ICommand? TileDocumentsHorizontal { get; set; }

    /// <summary>
    /// Gets or sets command to tile MDI documents vertically.
    /// </summary>
    ICommand? TileDocumentsVertical { get; set; }

    /// <summary>
    /// Gets or sets command to restore MDI documents to normal state.
    /// </summary>
    ICommand? RestoreDocuments { get; set; }

    /// <summary>
    /// Adds the specified document to this dock and activates it.
    /// </summary>
    /// <param name="document">The document to add.</param>
    void AddDocument(IDockable document);

    /// <summary>
    /// Adds the specified tool to this dock and activates it.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    void AddTool(IDockable tool);
}
