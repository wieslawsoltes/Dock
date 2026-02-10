// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Defines creation and lifecycle hooks for ItemsSource-generated dock item containers.
/// </summary>
public interface IDockItemContainerGenerator
{
    /// <summary>
    /// Creates a container for a document source item.
    /// </summary>
    /// <param name="dock">The source-backed document dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    /// <returns>The generated container, or <c>null</c> to skip generation for this item.</returns>
    IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index);

    /// <summary>
    /// Prepares a generated document container before it is added to layout.
    /// </summary>
    /// <param name="dock">The source-backed document dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index);

    /// <summary>
    /// Clears a generated document container before it is removed.
    /// </summary>
    /// <param name="dock">The source-backed document dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item being removed, when known.</param>
    void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item);

    /// <summary>
    /// Creates a container for a tool source item.
    /// </summary>
    /// <param name="dock">The source-backed tool dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    /// <returns>The generated container, or <c>null</c> to skip generation for this item.</returns>
    IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index);

    /// <summary>
    /// Prepares a generated tool container before it is added to layout.
    /// </summary>
    /// <param name="dock">The source-backed tool dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index);

    /// <summary>
    /// Clears a generated tool container before it is removed.
    /// </summary>
    /// <param name="dock">The source-backed tool dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item being removed, when known.</param>
    void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item);
}
