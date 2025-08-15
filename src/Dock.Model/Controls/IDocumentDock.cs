// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Windows.Input;
using Dock.Model.Core;
using Dock.Model;

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
    /// Gets or sets document tabs layout.
    /// </summary>
    DocumentTabLayout TabsLayout { get; set; }

    /// <summary>
    /// Gets or sets document presentation mode.
    /// </summary>
    DocumentPresentation Presentation { get; set; }

    /// <summary>
    /// Arranges document windows in a cascading layout (MDI only).
    /// </summary>
    void CascadeDocuments();

    /// <summary>
    /// Tiles document windows horizontally (MDI only).
    /// </summary>
    void TileDocumentsHorizontally();

    /// <summary>
    /// Tiles document windows vertically (MDI only).
    /// </summary>
    void TileDocumentsVertically();

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
