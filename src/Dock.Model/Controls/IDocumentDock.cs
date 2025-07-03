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
    public bool EnableWindowDrag { get; set;}
}
