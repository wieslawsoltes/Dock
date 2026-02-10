// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Provides hooks to create, prepare, and clear ItemsSource-generated
/// document and tool containers.
/// </summary>
public interface IDockItemContainerGenerator
{
    /// <summary>
    /// Creates a document container for the specified source item.
    /// </summary>
    /// <param name="dock">The document items-source dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The item index when known; otherwise -1.</param>
    /// <returns>The generated container, or <c>null</c> to skip generation.</returns>
    IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index);

    /// <summary>
    /// Prepares a document container after creation.
    /// </summary>
    /// <param name="dock">The document items-source dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The item index when known; otherwise -1.</param>
    void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index);

    /// <summary>
    /// Clears a previously generated document container.
    /// </summary>
    /// <param name="dock">The document items-source dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item that produced the container.</param>
    void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item);

    /// <summary>
    /// Creates a tool container for the specified source item.
    /// </summary>
    /// <param name="dock">The tool items-source dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The item index when known; otherwise -1.</param>
    /// <returns>The generated container, or <c>null</c> to skip generation.</returns>
    IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index);

    /// <summary>
    /// Prepares a tool container after creation.
    /// </summary>
    /// <param name="dock">The tool items-source dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The item index when known; otherwise -1.</param>
    void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index);

    /// <summary>
    /// Clears a previously generated tool container.
    /// </summary>
    /// <param name="dock">The tool items-source dock.</param>
    /// <param name="container">The generated container.</param>
    /// <param name="item">The source item that produced the container.</param>
    void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item);
}
