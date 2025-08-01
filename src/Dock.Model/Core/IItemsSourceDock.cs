// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections;

namespace Dock.Model.Core;

/// <summary>
/// Interface for docks that support ItemsSource-based document generation.
/// </summary>
public interface IItemsSourceDock
{
    /// <summary>
    /// Gets the ItemsSource collection.
    /// </summary>
    IEnumerable? ItemsSource { get; }

    /// <summary>
    /// Checks if a document was generated from ItemsSource.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document was generated from ItemsSource, false otherwise.</returns>
    bool IsDocumentFromItemsSource(IDockable document);

    /// <summary>
    /// Removes an item from the ItemsSource collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    bool RemoveItemFromSource(object? item);
}