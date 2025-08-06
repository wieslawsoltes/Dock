// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Grid splitter dock contract.
/// </summary>
[RequiresDataTemplate]
public interface IGridDockSplitter : IDockable
{
    /// <summary>
    /// Gets or sets resize direction.
    /// </summary>
    GridResizeDirection ResizeDirection { get; set; }
}
