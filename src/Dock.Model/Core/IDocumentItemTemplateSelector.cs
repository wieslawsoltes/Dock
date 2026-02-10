// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Selects template content for generated document containers.
/// </summary>
public interface IDocumentItemTemplateSelector
{
    /// <summary>
    /// Selects template content for a source item.
    /// </summary>
    /// <param name="dock">The source-backed document dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    /// <returns>Template content for the generated container, or <c>null</c> to use dock defaults.</returns>
    object? SelectTemplate(IItemsSourceDock dock, object item, int index);
}
