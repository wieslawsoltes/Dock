// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Dock.Model.Controls;

namespace Dock.Avalonia.Mdi;

/// <summary>
/// Provides the contract for arranging MDI document windows.
/// </summary>
public interface IMdiLayoutManager
{
    /// <summary>
    /// Arranges the provided entries within the available bounds.
    /// </summary>
    /// <param name="entries">The entries to arrange.</param>
    /// <param name="finalSize">The available size.</param>
    void Arrange(IReadOnlyList<MdiLayoutEntry> entries, Size finalSize);

    /// <summary>
    /// Calculates bounds for a drag operation.
    /// </summary>
    /// <param name="document">The document being dragged.</param>
    /// <param name="startBounds">The starting bounds.</param>
    /// <param name="delta">The drag delta.</param>
    /// <param name="finalSize">The available size.</param>
    /// <param name="entries">The entries participating in layout.</param>
    /// <returns>The updated bounds.</returns>
    Rect GetDragBounds(IMdiDocument document, Rect startBounds, Vector delta, Size finalSize, IReadOnlyList<MdiLayoutEntry> entries);

    /// <summary>
    /// Calculates bounds for a resize operation.
    /// </summary>
    /// <param name="document">The document being resized.</param>
    /// <param name="startBounds">The starting bounds.</param>
    /// <param name="delta">The resize delta.</param>
    /// <param name="direction">The resize direction.</param>
    /// <param name="finalSize">The available size.</param>
    /// <param name="entries">The entries participating in layout.</param>
    /// <returns>The updated bounds.</returns>
    Rect GetResizeBounds(IMdiDocument document, Rect startBounds, Vector delta, MdiResizeDirection direction, Size finalSize, IReadOnlyList<MdiLayoutEntry> entries);

    /// <summary>
    /// Updates Z-order based on the active document.
    /// </summary>
    /// <param name="documents">The documents to order.</param>
    /// <param name="activeDocument">The active document.</param>
    void UpdateZOrder(IReadOnlyList<IMdiDocument> documents, IMdiDocument? activeDocument);
}
