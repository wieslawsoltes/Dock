// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Selects template content for generated tool containers.
/// </summary>
public interface IToolItemTemplateSelector
{
    /// <summary>
    /// Selects template content for a source item.
    /// </summary>
    /// <param name="dock">The source-backed tool dock.</param>
    /// <param name="item">The source item.</param>
    /// <param name="index">The source index when known; otherwise -1.</param>
    /// <returns>Template content for the generated container, or <c>null</c> to use dock defaults.</returns>
    object? SelectTemplate(IToolItemsSourceDock dock, object item, int index);
}
