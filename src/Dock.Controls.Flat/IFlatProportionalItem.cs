// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Controls.Flat;

/// <summary>
/// Describes an item that can participate in flat proportional layout.
/// </summary>
public interface IFlatProportionalItem
{
    /// <summary>
    /// Gets the unique stable identity key used to reuse visuals across layout rebuilds.
    /// </summary>
    object Key { get; }

    /// <summary>
    /// Gets the content assigned to the generated presenter for leaf items.
    /// </summary>
    object? Content { get; }

    /// <summary>
    /// Gets or sets the active proportional size.
    /// </summary>
    double Proportion { get; set; }

    /// <summary>
    /// Gets or sets the remembered proportional size used when the item is collapsed.
    /// </summary>
    double CollapsedProportion { get; set; }

    /// <summary>
    /// Gets the minimum item width.
    /// </summary>
    double MinWidth { get; }

    /// <summary>
    /// Gets the minimum item height.
    /// </summary>
    double MinHeight { get; }

    /// <summary>
    /// Gets the maximum item width.
    /// </summary>
    double MaxWidth { get; }

    /// <summary>
    /// Gets the maximum item height.
    /// </summary>
    double MaxHeight { get; }

    /// <summary>
    /// Gets whether the item can collapse when empty.
    /// </summary>
    bool IsCollapsable { get; }

    /// <summary>
    /// Gets whether the item currently has no visible content.
    /// </summary>
    bool IsEmpty { get; }
}
