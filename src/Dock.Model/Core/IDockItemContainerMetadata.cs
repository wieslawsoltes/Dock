// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Optional presentation metadata for source-generated dockables.
/// </summary>
public interface IDockItemContainerMetadata
{
    /// <summary>
    /// Gets or sets the generated container theme metadata.
    /// The value can be a theme instance or a theme resource key.
    /// </summary>
    object? ItemContainerTheme { get; set; }

    /// <summary>
    /// Gets or sets metadata describing the selector used to resolve template content.
    /// </summary>
    object? ItemTemplateSelector { get; set; }
}
