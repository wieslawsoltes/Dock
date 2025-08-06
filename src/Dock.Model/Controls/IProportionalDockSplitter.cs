// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Proportional dock splitter contract.
/// </summary>
[RequiresDataTemplate]
public interface IProportionalDockSplitter : IDockable
{
    /// <summary>
    /// Gets or sets whether the splitter allows resizing.
    /// </summary>
    bool CanResize { get; set; }

    /// <summary>
    /// Gets or sets whether resizing occurs only after the pointer is released.
    /// When true a drag preview is shown during movement.
    /// </summary>
    bool ResizePreview { get; set; }
}
