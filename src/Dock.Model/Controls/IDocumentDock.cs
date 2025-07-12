// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Document dock contract.
/// </summary>
public interface IDocumentDock : IDock
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
    /// Adds the specified document to this dock and activates it.
    /// </summary>
    /// <param name="document">The document to add.</param>
    void AddDocument(IDockable document);

    /// <summary>
    /// Adds the specified tool to this dock and activates it.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    void AddTool(IDockable tool);

    /// <summary>
    /// Starts flashing the specified dockable's tab.
    /// </summary>
    /// <param name="dockable">The dockable to flash.</param>
    void FlashDockable(IDockable dockable);

    /// <summary>
    /// Stops flashing the specified dockable's tab.
    /// </summary>
    /// <param name="dockable">The dockable to stop flashing.</param>
    void StopFlashingDockable(IDockable dockable);
}
