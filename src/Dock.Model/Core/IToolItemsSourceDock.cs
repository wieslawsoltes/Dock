// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections;

namespace Dock.Model.Core;

/// <summary>
/// Interface for docks that support ItemsSource-based tool generation.
/// </summary>
public interface IToolItemsSourceDock
{
    /// <summary>
    /// Gets the ItemsSource collection.
    /// </summary>
    IEnumerable? ItemsSource { get; }

    /// <summary>
    /// Gets the item container generator used for source-backed tools.
    /// </summary>
    IDockItemContainerGenerator? ItemContainerGenerator { get; }

    /// <summary>
    /// Gets or sets per-dock container theme metadata used for generated tools.
    /// The value can be a theme instance or a theme resource key.
    /// </summary>
    object? ToolItemContainerTheme { get; set; }

    /// <summary>
    /// Gets or sets the per-dock template selector for generated tools.
    /// </summary>
    IToolItemTemplateSelector? ToolItemTemplateSelector { get; set; }

    /// <summary>
    /// Checks if a tool was generated from ItemsSource.
    /// </summary>
    /// <param name="tool">The tool to check.</param>
    /// <returns>True if the tool was generated from ItemsSource, false otherwise.</returns>
    bool IsToolFromItemsSource(IDockable tool);

    /// <summary>
    /// Removes an item from the ItemsSource collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    bool RemoveItemFromSource(object? item);
}
