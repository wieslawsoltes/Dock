// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Controls.Flat;

/// <summary>
/// Describes a splitter item for flat proportional layout.
/// </summary>
public interface IFlatProportionalSplitter : IFlatProportionalItem
{
    /// <summary>
    /// Gets whether the splitter can resize adjacent items.
    /// </summary>
    bool CanResize { get; }

    /// <summary>
    /// Gets whether resize should be previewed until pointer release.
    /// </summary>
    bool ResizePreview { get; }
}
